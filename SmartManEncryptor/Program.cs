using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

using SmartManEncryptor;

using System.Reflection.Metadata;
using System.Xml.Linq;

internal class Program
{
    // Call SalLoadAppAndWait( sSmartPath|| 'PaySlip.Exe '|| sTempPath ||sEmpCdArray[nNumber]||'E.pdf ' || sTempPath ||sEmpCdArray[nNumber]||'.pdf '||sPassWordArray[nNumber], Window_NotVisible, nReturn)    
    // SmartManEncryptor.exe -i 0000MAXE.pdf -o 0000MAXE_encrypted.pdf -p A123456789 -c false -l logs -d 90 -r true
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
        SmartManEncryptorLog log = new SmartManEncryptorLog(logFolder);
        log.LogText("SmartManEncryptor started.");
        try
        {
            // Parse command-line arguments
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-i":
                        inputFile = args[++i];
                        break;
                    case "-o":
                        outputFile = args[++i];
                        break;
                    case "-p":
                        password = args[++i];
                        break;
                    case "-c":
                        compress = true;
                        break;
                    case "-l":
                        logFolder = args[++i];
                        log = new SmartManEncryptorLog(logFolder);
                        break;
                    case "-d":
                        if (int.TryParse(args[++i], out int days))
                            deleteOlderThanDays = days;
                        else
                            log.LogError($"Invalid value for -d option: {args[i]}");
                        break;
                    case "-r":
                        if (bool.TryParse(args[++i], out bool delete))
                            deleteOriginalFile = delete;
                        else
                            log.LogError($"Invalid value for -r option: {args[i]}");
                        break;
                    default:
                        log.LogError($"Invalid argument: {args[i]}");
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
        log.LogText($"Input file: {inputFile}");
        log.LogText($"Output file: {outputFile}");
        log.LogText(password.Length > 0 ? "Password: ********" : "Password: (empty)");
        log.LogText($"Compress: {compress}");
        log.LogText($"Log folder: {logFolder}");
        log.LogText($"Delete logs older than {deleteOlderThanDays} days");
        log.LogText($"Delete original file: {deleteOriginalFile}");

        // Check if required arguments are provided
        if (string.IsNullOrEmpty(inputFile) || string.IsNullOrEmpty(outputFile) || string.IsNullOrEmpty(password))
        {
            log.LogError($"Invalid arguments. Usage: SmartManEncryptor.exe -i inputFilePath -o outputFilePath -p password [-c] [-l logFolder] [-d deleteOlderThanDays] [-r deleteOriginalFile]");
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
        }
        catch (Exception ex)
        {
            log.LogError($"Error occurred while deleting old logs.", ex);
            return;
        }
        // Encrypt PDF
        try
        {
            log.LogText($"Encrypting PDF: {inputFile}");
            using (var inputDocument = PdfReader.Open(inputFile, PdfDocumentOpenMode.Import))
            {
                using (var outputDocument = new PdfDocument())
                {
                    foreach (var page in inputDocument.Pages)
                    {
                        outputDocument.AddPage(page);
                    }
                    if (compress)
                    {
                        outputDocument.Options.CompressContentStreams = true;
                        outputDocument.Options.NoCompression = false;
                        outputDocument.Options.FlateEncodeMode = PdfFlateEncodeMode.BestCompression;
                        outputDocument.Options.UseFlateDecoderForJpegImages = PdfUseFlateDecoderForJpegImages.Automatic;
                    }
                    outputDocument.SecuritySettings.UserPassword = password;
                    outputDocument.SecuritySettings.OwnerPassword = password;
                    outputDocument.SecuritySettings.PermitAnnotations = false;
                    outputDocument.SecuritySettings.PermitAssembleDocument = false;
                    outputDocument.SecuritySettings.PermitExtractContent = false;
                    outputDocument.SecuritySettings.PermitFormsFill = false;
                    outputDocument.SecuritySettings.PermitFullQualityPrint = false;
                    outputDocument.SecuritySettings.PermitModifyDocument = false;
                    outputDocument.SecuritySettings.PermitPrint = false;
                    outputDocument.Save(outputFile);
                }
                log.LogText($"PDF encrypted: {outputFile}");
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
                log.LogText($"Original file deleted: {inputFile}");
            }
            catch (Exception ex)
            {
                log.LogError($"Error occurred while deleting original file.", ex);
                return;
            }
        }
    }
}

