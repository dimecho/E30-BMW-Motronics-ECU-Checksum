using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

class MainClass
{
    [DllImport("kernel32.dll")]
    public static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput,int wAttributes);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GetStdHandle(uint nStdHandle);
	public static int Checksum1 = 0;
	public static int Checksum2 = 0;
	public static int Checksum_Offset = 46367; //0xB51F
	public static int Checksum_Final = 0;
	public static string Checksum_HEX = "";
    public static IntPtr hConsole = GetStdHandle(0xfffffff5);
	
	[STAThread]
    static void Main()
    {
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.WriteLine("Version: 1.1");
		Console.WriteLine("\n-------------------------------------------------------");
		Console.WriteLine("\nBMW E30 325i - M20 Engine - DME 173 Checksum Calculator");
		Console.WriteLine("\n-------------------------------------------------------\n");
		Console.WriteLine("press 'ENTER' to begin");
		Console.ReadLine();
		Console.Clear();
		SetConsoleTextAttribute(hConsole, 10); //green

        OpenFileDialog dlg = new OpenFileDialog();
		dlg.Filter = "BIN|*.bin";
		
		Console.Clear();
        if (dlg.ShowDialog() == DialogResult.OK)
        {	
			FileStream fSTR;
            try
            {
                fSTR = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.None);
            
				byte[] someBytes = new byte[16];
				long fAddress = 0;
				int cnt;
				while ((cnt = fSTR .Read(someBytes, 0, 16)) > 0)
				{
				if (fAddress > 7936 && fAddress < 8192){
					SetConsoleTextAttribute(hConsole, 12); //red
				}else{
					SetConsoleTextAttribute(hConsole, 10); //green
				}
					Console.WriteLine(ContructLine(fAddress, someBytes, cnt));
					Thread.Sleep(5);
					fAddress += 16;
				}
				fSTR.Close();
				Console.WriteLine();
			
				SetConsoleTextAttribute(hConsole, 15); //white
				fSTR = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.None);
				
				//Read first half Checksum Range 1
				byte[] ByteArray1=new byte[7936];
				int nBytesRead=fSTR.Read(ByteArray1, 0, 7936);
				string hex = BitConverter.ToString(ByteArray1);
				string[] words = hex.Split('-');
				foreach (string word in words)
				{
					int a = int.Parse(word, NumberStyles.HexNumber);
					Checksum1 +=a;
				}
				Console.WriteLine("Checksum Range 1: " + Checksum1);

				//Skip 257 bytes
				byte[] ByteArray2=new byte[256];
				nBytesRead=fSTR.Read(ByteArray2, 0, 256);
					 
				//Read second half Checksum Range 2
				byte[] ByteArray3=new byte[24576];
				nBytesRead=fSTR.Read(ByteArray3, 0, 24576);
			
				hex = BitConverter.ToString(ByteArray3);
				words = hex.Split('-');

				foreach (string word in words)
				{
					int a = int.Parse(word, NumberStyles.HexNumber);
					Checksum2 += a;
				}
				
				Checksum_Final = Checksum1 + Checksum2 + Checksum_Offset;
				Checksum_HEX = Checksum_Final.ToString("X").Substring(2,4);
				
				Console.WriteLine("Checksum Range 2: " + Checksum2);
				Console.WriteLine("_______________");

				Console.Write("Final Checksum  : " + Checksum_Final + " ---> [");
				SetConsoleTextAttribute(hConsole, 12); //red
				Console.Write(Checksum_HEX);
				SetConsoleTextAttribute(hConsole, 15); //white
				Console.Write("]");
				fSTR.Close();
				
				Console.WriteLine("\nSave this checksum to current file? (Y/N)");
				string q = Console.ReadLine();
				
				if (q.ToUpper() == "Y")
                {
					//byte[] patch = { 0xEB, 0xFE };
					byte[] patch = { Convert.ToByte(Checksum_HEX.Substring(0, 2), 16), Convert.ToByte(Checksum_HEX.Substring(2, 2), 16) };
					
					BinaryWriter bw = new BinaryWriter(File.Open(dlg.FileName, FileMode.Open, FileAccess.ReadWrite));
					bw.BaseStream.Seek(0x1F00, SeekOrigin.Begin);
					bw.Write(patch);
					bw.Close();
					
					SetConsoleTextAttribute(hConsole, 10); //green
					Console.WriteLine("\nChecksum saved sucessfully.");
					Console.ReadKey();
				}
			}
            catch (Exception ex)
            {
				SetConsoleTextAttribute(hConsole, 12); //red
                Console.WriteLine(">>> ERROR: {0}", ex.Message);
				Console.ReadKey();
            }
        }
	}
	
	static string ContructLine(long Adr, byte[] buf, int count)
    {
        string str = String.Format("{0:X4} {1:X4}  ", Adr / 65536, Adr);
        for (int i = 0; i < 16; i++)
        {
            str += (i < count) ? String.Format("{0:X2}", buf[i]) : "  ";
            str += " ";
        }
        str += " ";
         //convert file to readable chars if possible
        for (int i = 0; i < 16; i++)
        {
            char ch = (i < count) ? Convert.ToChar(buf[i]) : ' ';
            str += Char.IsControl(ch) ? "." : ch.ToString();
        }
        return str;
    }
}