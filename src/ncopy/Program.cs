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

						if (String.IsNullOrWhiteSpace(source))
							source = String.Empty;

						if (String.IsNullOrWhiteSpace(destination))
							destination = Directory.GetCurrentDirectory();

						if (Directory.Exists(source))
							CopyDirectory(source, destination, true, false, true, ref count);
						else if (File.Exists(source))
							CopyFile(source, destination, true, true, ref count);
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

		private static void CopyDirectory(string source, string destination, bool overwrite, bool copyEmptyDirectories, bool copySubDirectories, ref int count)
		{
			DirectoryInfo sourceDirectory = new DirectoryInfo(source);

			if (!copyEmptyDirectories && !sourceDirectory.EnumerateFileSystemInfos().Any())
				return;

			foreach (FileInfo fileInfo in sourceDirectory.GetFiles())
				CopyFile(fileInfo.FullName, Path.Combine(destination, fileInfo.Name), overwrite, true, ref count);
			
			if (!copySubDirectories)
				return;

			foreach (DirectoryInfo directoryInfo in sourceDirectory.GetDirectories())
				CopyDirectory(directoryInfo.FullName, Path.Combine(destination, directoryInfo.Name), overwrite, copyEmptyDirectories, copySubDirectories, ref count);
		}

		private static void CopyFile(string source, string destination, bool overwrite, bool createMissingDirectories, ref int count)
		{
			FileInfo sourceFile = new FileInfo(source);

			//Console.WriteLine("Destination: " + destination);
			//Console.WriteLine("Directory Name: " + Path.GetDirectoryName(destination));
			//Console.WriteLine("File Name: " + Path.GetFileName(destination));
			//Console.WriteLine("Extension: " + Path.GetExtension(destination));

			if (String.IsNullOrWhiteSpace(Path.GetExtension(destination)))
			{
				// destination is a directory
				DirectoryInfo destinationDirectory = new DirectoryInfo(destination);

				if (createMissingDirectories && !destinationDirectory.Exists)
					destinationDirectory.Create();

				string destinationFileName = Path.Combine(destinationDirectory.FullName, sourceFile.Name);
				sourceFile.CopyTo(destinationFileName, overwrite);
			}
			else
			{
				// destination is a file
				FileInfo destinationFile = new FileInfo(destination);

				if (createMissingDirectories && !destinationFile.Directory.Exists)
					destinationFile.Directory.Create();

				string destinationFileName = destinationFile.FullName;
				sourceFile.CopyTo(destinationFileName, overwrite);
			}

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