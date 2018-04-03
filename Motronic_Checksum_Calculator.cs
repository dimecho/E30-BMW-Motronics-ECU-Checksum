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
	public static int Checksum_Final = 0;
	public static string Checksum_HEX = "";
	public static IntPtr hConsole = GetStdHandle(0xfffffff5);
	public static FileStream fSTR;
	
	[STAThread]
    static void Main()
    {
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.WriteLine("Version: 1.1.8");
		Console.WriteLine("\n-------------------------------------------");
		Console.WriteLine("[Description]");
		Console.WriteLine("\nMotronic Checksum Calculator.");
		Console.WriteLine("\nUntested in real life. Provided AS-IS");
		Console.WriteLine("\n-------------------------------------------\n");
		
		Console.WriteLine("Available versions:\n");
		SetConsoleTextAttribute(hConsole, 8); //green
		Console.WriteLine("[1] Motronic 1.3 (BMW E30 M20 ECU 0261200173)");
		Console.WriteLine("[2] Motronic 3.1 (BMW E36 M50 ECU 0261200402 and 1267357098)");
		Console.WriteLine("[3] Motronic 1.5.5");
		Console.WriteLine("[4] Motronic 2.9 (VW VR6 93-95 Golf/Jetta)");
		Console.WriteLine("[5] Motronic 3.1 (late version)");
		Console.WriteLine("[6] Motronic 3.1 (early version) 3.3, 3.3.1");
		Console.WriteLine("[7] Motronic 4.4 (Volvo, Saab)");

		SetConsoleTextAttribute(hConsole, 15); //green
		Console.WriteLine("\nSelect Option (1-8):");
		
		string select = Console.ReadLine();
		Console.Clear();
		SetConsoleTextAttribute(hConsole, 10); //green

        OpenFileDialog dlg = new OpenFileDialog();
		dlg.Filter = "BIN|*.bin";
		
		Console.Clear();
        if (dlg.ShowDialog() == DialogResult.OK)
        {	
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
					//Thread.Sleep(4);
					fAddress += 16;
				}
				fSTR.Close();
				Console.WriteLine();
				
				if (select == "1"){
					Motronic1_3(dlg.FileName);
				}else if(select == "2"){
					Motronic3_1_M50B25(dlg.FileName);
				}else if(select == "3"){
					Motronic1_5_5(dlg.FileName);
				}else if(select == "4"){
					Motronic2_9(dlg.FileName);
				}else if(select == "5"){
					Motronic3_1_late(dlg.FileName);
				}else if(select == "6"){
					Motronic3_1_early(dlg.FileName);
				}else if(select == "7"){
					Motronic4_4(dlg.FileName);
				}else if(select == "8"){
					Motronic7_1(dlg.FileName);
				}else{
					Console.WriteLine("Selected Motronic version {not found}");
				}
			}
            catch (Exception ex)
            {
				SetConsoleTextAttribute(hConsole, 12); //red
                Console.WriteLine(">>> ERROR: {0}", ex.Message + " " + ex.StackTrace);
				Console.ReadKey();
            }
        }
	}
	
	static void Motronic1_3(string filename)
    {
		SetConsoleTextAttribute(hConsole, 15); //white
		fSTR = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
				
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
				
		Checksum_Final = Checksum1 + Checksum2 + 46367; //0xB51F
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
				
		if (q.ToUpper() == "Y"){
			//byte[] patch = { 0xEB, 0xFE };
			byte[] patch = { Convert.ToByte(Checksum_HEX.Substring(0, 2), 16), Convert.ToByte(Checksum_HEX.Substring(2, 2), 16) };
					
			BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0x1F00, SeekOrigin.Begin);
			bw.Write(patch);
			bw.Close();
					
			SetConsoleTextAttribute(hConsole, 10); //green
			Console.WriteLine("\nChecksum saved sucessfully.");
			Console.ReadKey();
		}
	}
		
	static void Motronic1_5_5(string filename)
    {
		SetConsoleTextAttribute(hConsole, 15); //white
		fSTR = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
				
		//Read first half Checksum Range 1
		byte[] ByteArray1=new byte[65528];
		int nBytesRead=fSTR.Read(ByteArray1, 0, 65528); //0000 - FFF7
		string hex = BitConverter.ToString(ByteArray1);
		string[] words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum1 +=a;
		}
		Console.Write("\nChecksum Range 1 : " + Checksum1);
		
		//skip
		byte[] ByteArray2=new byte[4];
		nBytesRead=fSTR.Read(ByteArray2, 0, 4);
		//string hex1 = BitConverter.ToString(ByteArray2);
		//Console.Write("\n Skip 1 : " + hex1);
		Checksum1 +=510; //FF FF
		
		
		//Read first second Checksum Range 2
		byte[] ByteArray3=new byte[65532];
		nBytesRead=fSTR.Read(ByteArray3, 0, 65532); //10000 - 1FFF7
		hex = BitConverter.ToString(ByteArray3);
		words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum2 +=a;
		}
		
		Console.Write("\nChecksum Range 2 : " + Checksum2);
		
		//skip
		byte[] ByteArray4=new byte[4];
		nBytesRead=fSTR.Read(ByteArray4, 0, 4);
		//string hex2 = BitConverter.ToString(ByteArray4);
		//Console.Write("\n Skip 2 : " + hex2);
		Checksum2 +=510; //FF FF

		//Read first half Checksum Range 1
		byte[] ByteArray5=new byte[4];
		nBytesRead=fSTR.Read(ByteArray5, 0, 4);
		hex = BitConverter.ToString(ByteArray5);
		words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum2 +=a;
		}
		
		Checksum1 = Checksum1 + Checksum2;
		
		Checksum_Final = 65535 - int.Parse(Checksum1.ToString("X").Substring(Checksum1.ToString().Length-5,4), NumberStyles.HexNumber); //Complement
		Checksum_HEX = Checksum1.ToString("X").Substring(Checksum1.ToString().Length-5,4) + Checksum_Final.ToString("X");
		//Checksum1 = int.Parse(Checksum_HEX, NumberStyles.HexNumber);
		
		Console.Write("\nFinal Checksum : " + Checksum1 + " ---> [");
		SetConsoleTextAttribute(hConsole, 12); //red
		Console.Write(Checksum_HEX);
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.Write("]");
		
		Console.WriteLine("\nSave this checksum to current file? (Y/N)");
		string q = Console.ReadLine();
				
		if (q.ToUpper() == "Y"){

			byte[] patch = { Convert.ToByte(Checksum_HEX.Substring(0,2), 16), Convert.ToByte(Checksum_HEX.Substring(2, 2), 16), Convert.ToByte(Checksum_HEX.Substring(4, 2), 16) , Convert.ToByte(Checksum_HEX.Substring(6, 2), 16)};	
			BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0xFFF8, SeekOrigin.Begin);
			bw.Write(patch);
			bw.Close();
				
			bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0x1FFF8, SeekOrigin.Begin);
			bw.Write(patch);
			bw.Close();
			
			SetConsoleTextAttribute(hConsole, 10); //green
			Console.WriteLine("\nChecksum saved sucessfully.");
			Console.ReadKey();
		}
	}
	
	static void Motronic2_9(string filename)
    {
		SetConsoleTextAttribute(hConsole, 15); //white
		fSTR = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
				
		//Read first half Checksum Range 1
		byte[] ByteArray1=new byte[52992];
		int nBytesRead=fSTR.Read(ByteArray1, 0, 52992);
		string hex = BitConverter.ToString(ByteArray1);
		string[] words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum1 +=a;
		}
		fSTR.Close();
		
		Console.Write("\nChecksum : " + Checksum1 + " ---> [");
		SetConsoleTextAttribute(hConsole, 12); //red
		Console.Write(Checksum1.ToString("X").Substring(Checksum1.ToString().Length-5,4));
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.Write("]");

		Console.WriteLine("\nSave this checksum to current file? (Y/N)");
		string q = Console.ReadLine();
				
		if (q.ToUpper() == "Y"){
			int a = Checksum1.ToString().Length-5;
			byte[] patch = { Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(0, 2), 16), Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(2, 2), 16) };	
			BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0xCF00, SeekOrigin.Begin);
			bw.Write(patch);
			bw.Close();
					
			SetConsoleTextAttribute(hConsole, 10); //green
			Console.WriteLine("\nChecksum saved sucessfully.");
			Console.ReadKey();
		}
	}
	
	static void Motronic4_4(string filename)
    {
				SetConsoleTextAttribute(hConsole, 15); //white
		fSTR = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
				
		//Read first half Checksum Range 1
		byte[] ByteArray1=new byte[65280];
		int nBytesRead=fSTR.Read(ByteArray1, 0, 65280);
		string hex = BitConverter.ToString(ByteArray1);
		string[] words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum1 +=a;
		}
		
		//Skip 2 bytes
		byte[] ByteArray2=new byte[256];
		nBytesRead=fSTR.Read(ByteArray2, 0, 256);
					 
		//Read second half Checksum Range 2
		byte[] ByteArray3=new byte[65280];
		nBytesRead=fSTR.Read(ByteArray3, 0, 65280);
			
		hex = BitConverter.ToString(ByteArray3);
		words = hex.Split('-');

		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum2 += a;
		}
		fSTR.Close();
		
		Console.Write("\nChecksum 1: " + Checksum1 + " ---> [");
		SetConsoleTextAttribute(hConsole, 12); //red
		Console.Write(Checksum1.ToString("X").Substring(Checksum1.ToString().Length-5,4));
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.Write("]");
	
		Console.Write("\nChecksum 2: " + Checksum2 + " ---> [");
		SetConsoleTextAttribute(hConsole, 12); //red
		Console.Write(Checksum2.ToString("X").Substring(Checksum2.ToString().Length-5,4));
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.Write("]");
		
		Console.WriteLine("\nSave these checksums to current file? (Y/N)");
		string q = Console.ReadLine();
				
		if (q.ToUpper() == "Y"){

			int a = Checksum1.ToString().Length-5;
			byte[] patch = { Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(0, 2), 16), Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(0, 2), 16) };	
			BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0xFF00, SeekOrigin.Begin);
			bw.Write(patch);
			bw.Close();
			
			int b = Checksum2.ToString().Length-5;
			byte[] patch2 = { Convert.ToByte(Checksum2.ToString("X").Substring(b,4).Substring(0, 2), 16), Convert.ToByte(Checksum2.ToString("X").Substring(b,4).Substring(0, 2), 16) };	
			bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0x1FF00, SeekOrigin.Begin);
			bw.Write(patch2);
			bw.Close();
					
			SetConsoleTextAttribute(hConsole, 10); //green
			Console.WriteLine("\nChecksum saved sucessfully.");
			Console.ReadKey();
		}
	}
	
	static void Motronic3_1_M50B25(string filename)
    {
				SetConsoleTextAttribute(hConsole, 15); //white
		fSTR = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);

		//skip
		int Checksum_Offset = 0;
		byte[] ByteArray1=new byte[27393];
		int nBytesRead=fSTR.Read(ByteArray1, 0, 27393);
		string hex = BitConverter.ToString(ByteArray1);
		string[] words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum_Offset +=a;
		}
		
		//Read first half Checksum Range 1
		byte[] ByteArray2=new byte[4118];
		nBytesRead=fSTR.Read(ByteArray2, 0, 4118); //6B00 to 7B15
		hex = BitConverter.ToString(ByteArray2);
		words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum1 +=a;
		}
		
		//skip
		byte[] ByteArray3=new byte[2];
		nBytesRead=fSTR.Read(ByteArray3, 0, 2);
		
		//Read first half Checksum Range 2
		byte[] ByteArray4=new byte[32766];
		nBytesRead=fSTR.Read(ByteArray4, 0, 32766);
		hex = BitConverter.ToString(ByteArray4);
		words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum2 +=a;
		}
		
		fSTR.Close();

		Console.Write("\nChecksum of Range 1 (0x6B00 to 0x7B15): " + Checksum1 + " ---> [");
		SetConsoleTextAttribute(hConsole, 12); //red
		Console.Write(Checksum1.ToString("X").Substring(Checksum1.ToString().Length-5,4));
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.Write("]");
		
		Checksum_Final = Checksum_Offset + Checksum1 + Checksum2;
		
		Console.Write("\nChecksum of whole file (including new checksum): " + Checksum_Final + " ---> [");
		SetConsoleTextAttribute(hConsole, 12); //red
		Console.Write(Checksum_Final.ToString("X").Substring(Checksum1.ToString().Length-5,4));
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.Write("]");
	
		Console.WriteLine("\nSave this checksum to current file? (Y/N)");
		string q = Console.ReadLine();
				
		if (q.ToUpper() == "Y"){
			int a = Checksum1.ToString().Length-5;
			byte[] patch = { Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(0, 2), 16), Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(2, 2), 16) };	
			BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0x7B16, SeekOrigin.Begin);
			bw.Write(patch);
			bw.Close();
					
			SetConsoleTextAttribute(hConsole, 10); //green
			Console.WriteLine("\nChecksum saved sucessfully.");
			Console.ReadKey();
		}
	}
	
	static void Motronic3_1_late(string filename)
    {
		SetConsoleTextAttribute(hConsole, 15); //white
		fSTR = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
				
		//Read first half Checksum Range 1
		byte[] ByteArray1=new byte[32512];
		int nBytesRead=fSTR.Read(ByteArray1, 0, 32512);
		string hex = BitConverter.ToString(ByteArray1);
		string[] words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum1 +=a;
		}
		fSTR.Close();
		
		Console.Write("\nChecksum : " + Checksum1 + " ---> [");
		SetConsoleTextAttribute(hConsole, 12); //red
		Console.Write(Checksum1.ToString("X").Substring(Checksum1.ToString().Length-5,4));
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.Write("]");
	

		Console.WriteLine("\nSave this checksum to current file? (Y/N)");
		string q = Console.ReadLine();
				
		if (q.ToUpper() == "Y"){
			int a = Checksum1.ToString().Length-5;
			byte[] patch = { Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(0, 2), 16), Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(2, 2), 16) };	
			BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0x7F00, SeekOrigin.Begin);
			bw.Write(patch);
			bw.Close();
					
			SetConsoleTextAttribute(hConsole, 10); //green
			Console.WriteLine("\nChecksum saved sucessfully.");
			Console.ReadKey();
		}
	}
	
	static void Motronic3_1_early(string filename)
    {
				SetConsoleTextAttribute(hConsole, 15); //white
		fSTR = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.None);
				
		//Skip 27137 bytes
		byte[] ByteArray2=new byte[27137];
		int nBytesRead=fSTR.Read(ByteArray2, 0, 27137);
		
		//Read first half Checksum Range 1
		byte[] ByteArray1=new byte[31132];
		nBytesRead=fSTR.Read(ByteArray1, 0, 31132);
		string hex = BitConverter.ToString(ByteArray1);
		string[] words = hex.Split('-');
		foreach (string word in words)
		{
			int a = int.Parse(word, NumberStyles.HexNumber);
			Checksum1 +=a;
		}
		fSTR.Close();
		
		Console.Write("\nChecksum : " + Checksum1 + " ---> [");
		SetConsoleTextAttribute(hConsole, 12); //red
		Console.Write(Checksum1.ToString("X").Substring(Checksum1.ToString().Length-5,4));
		SetConsoleTextAttribute(hConsole, 15); //white
		Console.Write("]");
	

		Console.WriteLine("\nSave this checksum to current file? (Y/N)");
		string q = Console.ReadLine();
				
		if (q.ToUpper() == "Y"){
			int a = Checksum1.ToString().Length-5;
			byte[] patch = { Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(0, 2), 16), Convert.ToByte(Checksum1.ToString("X").Substring(a,4).Substring(2, 2), 16) };	
			BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Open, FileAccess.ReadWrite));
			bw.BaseStream.Seek(0x799C, SeekOrigin.Begin);
			bw.Write(patch);
			bw.Close();
					
			SetConsoleTextAttribute(hConsole, 10); //green
			Console.WriteLine("\nChecksum saved sucessfully.");
			Console.ReadKey();
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