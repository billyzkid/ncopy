using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ncopy
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			try
			{
				if (args.Length >= 1 && args.Length <= 3)
				{
					bool help = false;
					Encoding encoding = Encoding.UTF8;
					string source = null;
					string destination = null;

					foreach (string arg in args)
					{
						if (arg == "/?")
							help = true;
						else if (arg.StartsWith("/ENCODING", StringComparison.InvariantCultureIgnoreCase))
							encoding = Encoding.GetEncoding(arg.Split(':')[1]);
						else if (source == null)
							source = arg;
						else if (destination == null)
							destination = arg;
					}

					if (help)
					{
						Console.WriteLine("Copies files and directory trees.");
						Console.WriteLine();
						Console.WriteLine("NCOPY directory [destination]");
						Console.WriteLine("NCOPY file [destination]");
						Console.WriteLine("NCOPY file1[+file2[+file3]... destination /ENCODING:name");
						Environment.Exit(0);
					}
					else
					{
						int count = 0;

						if (string.IsNullOrWhiteSpace(source))
							source = string.Empty;
						if (string.IsNullOrWhiteSpace(destination))
							destination = Directory.GetCurrentDirectory();
						if (Directory.Exists(source))
							CopyDirectory(source, destination, true, true, ref count);
						else if (File.Exists(source))
							CopyFile(source, destination, true, ref count);
						else if (source.Split('+').All(file => File.Exists(file)))
							ConcatFiles(source.Split('+'), destination, encoding, true, ref count);

						Console.WriteLine("{0} File(s) copied", count);
						Environment.Exit(0);
					}
				}
				else
				{
					Console.WriteLine("Invalid number of parameters");
					Console.WriteLine("0 File(s) copied");
					Environment.Exit(1);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Environment.Exit(1);
			}
		}

		private static void CopyDirectory(string source, string destination, bool overwrite, bool copySubDirectories, ref int count)
		{
			DirectoryInfo sourceDirectory = new DirectoryInfo(source);
			DirectoryInfo destinationDirectory = new DirectoryInfo(destination);

			if (!destinationDirectory.Exists)
				destinationDirectory.Create();

			foreach (FileInfo fileInfo in sourceDirectory.GetFiles())
				CopyFile(fileInfo.FullName, Path.Combine(destination, fileInfo.Name), overwrite, ref count);
			
			if (!copySubDirectories)
				return;

			foreach (DirectoryInfo directoryInfo3 in sourceDirectory.GetDirectories())
				CopyDirectory(directoryInfo3.FullName, Path.Combine(destination, directoryInfo3.Name), overwrite, copySubDirectories, ref count);
		}

		private static void CopyFile(string source, string destination, bool overwrite, ref int count)
		{
			FileInfo sourceFile = new FileInfo(source);
			DirectoryInfo destinationDirectory = new DirectoryInfo(destination);

			if (destinationDirectory.Exists)
				destination = Path.Combine(destination, sourceFile.Name);

			sourceFile.CopyTo(destination, overwrite);

			Console.WriteLine(sourceFile.FullName);
			++count;
		}

		private static void ConcatFiles(string[] sources, string destination, Encoding encoding, bool overwrite, ref int count)
		{
			StringBuilder stringBuilder = new StringBuilder();

			foreach (string source in sources)
			{
				FileInfo sourceFile = new FileInfo(source);
				string sourceText = File.ReadAllText(source);
				stringBuilder.Append(sourceText);

				Console.WriteLine(sourceFile.FullName);
				++count;
			}

			string destinationText = stringBuilder.ToString();
			File.WriteAllText(destination, destinationText, encoding);
		}
	}
}