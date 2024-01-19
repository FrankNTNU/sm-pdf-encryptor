using System.IO.Compression;

using SmartManEncryptor;
using iTextSharp.text.pdf;
using System.Text;
internal class Program
{
    // Call SalLoadAppAndWait( sSmartPath|| 'PaySlip.Exe '|| sTempPath ||sEmpCdArray[nNumber]||'E.pdf ' || sTempPath ||sEmpCdArray[nNumber]||'.pdf '||sPassWordArray[nNumber], Window_NotVisible, nReturn)
    // sm-pdf-encrypt.exe -i my_file.pdf -o my_encrypted_file.pdf -p A123456789 -r
    private static void Main(string[] args)
    {
        // Default values
        string inputFile = "";
        string outputFile = "";
        string password = "";
        bool compress = false;
        string logFolder = "logs";
        int deleteOlderThanDays = 90;
        bool deleteOriginalFile = false;
        bool isVerbose = false;
        SmartManEncryptorLog log = new SmartManEncryptorLog(logFolder);
        log.LogText("sm-pdf-encrypt.exe started.");
        try
        {
            // Parse command-line arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-i" or "--input":
                        inputFile = args[++i];
                        break;
                    case "-o" or "--output":
                        outputFile = args[++i];
                        break;
                    case "-p" or "--password":
                        password = args[++i];
                        break;
                    case "-c" or "--compress":
                        compress = true;
                        break;
                    case "-l" or "--log-folder":
                        logFolder = args[++i];
                        log = new SmartManEncryptorLog(logFolder);
                        break;
                    case "-d" or "--delete-before-days":
                        if (int.TryParse(args[++i], out int days))
                            deleteOlderThanDays = days;
                        else
                            log.LogError($"Invalid value for -d option (should be a number): {args[i]}");
                        break;
                    case "-r" or "--remove-input-file":
                        deleteOriginalFile = true;
                        break;
                    case "-v" or "--verbose":
                            isVerbose = true;
                        break;
                    default:
                        log.LogError($"Invalid argument (Should be -i, -o, -p, -c, -l, -d, -r, or -v): {args[i]}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Error occurred while parsing command-line arguments.", ex);
            return;
        }
        // Print all arguments
        if (isVerbose)
        {
            log.LogText($"Input file: {inputFile}");
            log.LogText($"Output file: {outputFile}");
            // Generate * of the password's length
            string mask = new string('*', password.Length);
            log.LogText(password.Length > 0 ? $"Password: {mask}" : "Password: (empty)");
            log.LogText($"Compress: {compress}");
            log.LogText($"Log folder: {logFolder}");
            log.LogText($"Delete logs older than {deleteOlderThanDays} days");
            log.LogText($"Remove original file: {deleteOriginalFile}");
        }

        // Check if required arguments are provided
        if (string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFile) || string.IsNullOrEmpty(password))
        {
            log.LogError($"Invalid arguments. Some required arguments are missing.");
            return;
        }
        // Check if input file exists
        if (!File.Exists(inputFile))
        {
            log.LogError($"Input file does not exist: {inputFile}");
            return;
        }
        try
        {
            // Delete old logs
            log.DeleteOldLogs(deleteOlderThanDays);
            log.LogText($"Old logs deleted. Logs older than {deleteOlderThanDays} days are removed.");
        }
        catch (Exception ex)
        {
            log.LogError($"Error occurred while deleting old logs.", ex);
            return;
        }
        // Encrypt PDF
        try
        {
            string outputDirectory = Path.GetDirectoryName(outputFile) ?? "";

            if (!Directory.Exists(outputDirectory) && outputDirectory is not "")
            {
                Directory.CreateDirectory(outputDirectory);
                log.LogText($"Output directory created: {outputDirectory}");
            }
            log.LogText($"PDF to encrypt: {inputFile}. Size: {GetFileSizeInKB(inputFile)} KB");

            // Use Docotic.Pdf to encrypt PDF file (paid but good)
            //using (BitMiracle.Docotic.Pdf.PdfDocument doc = new BitMiracle.Docotic.Pdf.PdfDocument(inputFile))
            //{
            //    // Add password using BitMiracle.Docotic.Pdf
            //    doc.Save(outputFile, new BitMiracle.Docotic.Pdf.PdfSaveOptions { EncryptionHandler = new PdfStandardEncryptionHandler(password, password) }) ;
            //}
            //log.LogText($"PDF encrypted (Docotic.Pdf): {outputFile}. Size: {GetFileSizeInKB(outputFile)} KB");

            // Use Spire.PDF to encrypt PDF file (paid but good)
            //using (var document = new Spire.Pdf.PdfDocument())
            //{
            //    document.LoadFromFile(inputFile);
            //    document.Security.Encrypt(password);
            //    document.SaveToFile(outputFile);
            //}
            //log.LogText($"PDF encrypted (Spire): {outputFile}. Size: {GetFileSizeInKB(outputFile)} KB");

            // Use iTextSharp to encrypt PDF file
            log.LogText($"Encrypting file: {inputFile}");
            using (Stream input = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (Stream output = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(input);
                    // Create a stamper with compression settings
                    using (iTextSharp.text.pdf.PdfStamper stamper = new iTextSharp.text.pdf.PdfStamper(reader, output))
                    {
                        stamper.SetFullCompression();

                        // Set password protection
                        stamper.SetEncryption(
                            Encoding.UTF8.GetBytes(password),
                            Encoding.UTF8.GetBytes(password),
                            PdfWriter.ALLOW_SCREENREADERS,
                            PdfWriter.ENCRYPTION_AES_256
                        );
                    }
                    // PdfEncryptor.Encrypt(reader, output, true, password, password, PdfWriter.ALLOW_SCREENREADERS);
                }
            }
            log.LogText($"PDF encrypted: {outputFile}. Size: {GetFileSizeInKB(outputFile)} KB.");
            // Use PdfSharpCore to encrypt the PDF file (too large)
            //using (var inputDocument = PdfSharpCore.Pdf.IO.PdfReader.Open(inputFile, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.InformationOnly, accuracy: PdfSharpCore.Pdf.IO.enums.PdfReadAccuracy.Moderate))
            //{
            //    var document = new PdfSharpCore.Pdf.PdfDocument();
            //    foreach (var page in inputDocument.Pages)
            //    {
            //        document.AddPage(page);
            //    }
            //    // print document.FileSize
            //    log.LogText($"PDF file size before (PdfSharpCore): {document.FileSize} bytes");
            //    //document.Options.FlateEncodeMode = PdfSharpCore.Pdf.PdfFlateEncodeMode.BestCompression;
            //    //document.Options.NoCompression = false;
            //    //document.Options.CompressContentStreams = true;
            //    //document.Options.UseFlateDecoderForJpegImages = PdfSharpCore.Pdf.PdfUseFlateDecoderForJpegImages.Automatic;
            //    //document.Options.UseFlateDecoderForJpegImages = PdfSharpCore.Pdf.PdfUseFlateDecoderForJpegImages.Never;
            //    //document.SecuritySettings.UserPassword = password;
            //    //document.SecuritySettings.OwnerPassword = password;
            //    document.Save(outputFile);
            //    log.LogText($"PDF file size after (PdfSharpCore): {document.FileSize} bytes");
            //    document.Dispose();
            //    log.LogText($"PDF encrypted (PdfSharpCore): {outputFile}. Size: {GetFileSizeInKB(outputFile)} KB");
            //}

            // PdfSharp (too large)
            //using (var inputDocument = PdfSharp.Pdf.IO.PdfReader.Open(inputFile, PdfDocumentOpenMode.Import))
            //{
            //    using (var outputDocument = new PdfSharp.Pdf.PdfDocument())
            //    {
            //        foreach (var page in inputDocument.Pages)
            //        {
            //            outputDocument.AddPage(page);
            //        }
            //        //outputDocument.Options.CompressContentStreams = true;
            //        //outputDocument.Options.NoCompression = false;
            //        //outputDocument.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
            //        //outputDocument.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Automatic;
            //        //outputDocument.SecuritySettings.UserPassword = password;
            //        //outputDocument.SecuritySettings.OwnerPassword = password;
            //        outputDocument.Save(outputFile);
            //    }
            //    log.LogText($"PDF encrypted: {outputFile}. Size: {GetFileSizeInKB(outputFile)} KB");
            //}

            // Use System.IO.Compression to compress the output PDF file
            if (compress)
            {
                log.LogText($"Compressing file {outputFile}...");
                try
                {
                    // zip the file
                    string zipFileName = $"{outputFile}.zip";
                    // If the zip file already exists, delete it
                    if (File.Exists(zipFileName))
                    {
                        File.Delete(zipFileName);
                        log.LogText($"Existing zip file removed: {zipFileName}");
                    }
                    using (var zip = ZipFile.Open(zipFileName, ZipArchiveMode.Create))
                    {
                        zip.CreateEntryFromFile(outputFile, Path.GetFileName(outputFile), CompressionLevel.SmallestSize);
                    }
                    log.LogText($"PDF compressed: {zipFileName}. Size: {GetFileSizeInKB(zipFileName)} KB");
                }
                catch (Exception ex)
                {
                    log.LogError($"Error occurred while compressing the file: {outputFile}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Error occurred while encrypting PDF.", ex);
            return;
        }
        // Delete original file
        if (deleteOriginalFile)
        {
            try
            {
                File.Delete(inputFile);
                log.LogText($"Original file removed: {inputFile}");
                // Delete the encrypted file
                File.Delete(outputFile);
                log.LogText($"Encrypted file removed: {outputFile}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error occurred while removing original file.", ex);
                return;
            }
        }
        // Exit
        log.LogText("sm-pdf-encrypt.exe completed.");
    }
    // Get file size in KB helper
    private static long GetFileSizeInKB(string fileName) => new FileInfo(fileName).Length / 1024;
}