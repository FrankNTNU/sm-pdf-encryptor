using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Pdf.Security;

using SmartManEncryptor;

using System.Reflection.Metadata;

internal class Program
{
    /*
     * Call SalLoadAppAndWait( sSmartPath|| 'PaySlip.Exe '|| sTempPath ||sEmpCdArray[nNumber]||'E.pdf '
                || sTempPath ||sEmpCdArray[nNumber]||'.pdf '||sPassWordArray[nNumber], Window_NotVisible, nReturn)
     */
    // Arguments: inputFilePath, outputFilePath, password
    // Usage: SmartManEncryptor.exe "C:\Users\user\Desktop\test.pdf" "C:\Users\user\Desktop\test_encrypted.pdf" "password" false
    /*
     *  程式目的: 將指定檔案加密，也可以壓縮。(若選擇壓縮代表密碼為壓縮密碼)
        使用方式: SmartManEncryptor.exe “my_file.pdf” “encrypted_my_file.pdf” A123456789 false
        參數說明: SmartManEncryptor.exe (來源檔案路徑) (加密後的檔案路徑)       (密碼)	   (是否壓縮:預設false)
     */
    // SmartManEncryptor.exe 0000MAXE.pdf 0000MAXE_encrypted.pdf A123456789 false
    private static void Main(string[] args)
    {
        Config config = new Config();
        // A const file name // "smartman_encryptor_config.json"
        string configFileName = Path.Combine(Directory.GetCurrentDirectory(), "smartman_encryptor_config.json");

        SmartManEncryptorLog log = new SmartManEncryptorLog(config.LogDirectory);
        log.LogText("configFileName: " + configFileName);
        log.LogText("SmartManEncryptor started.");
        try
        {
            // Read config from config.json
            if (File.Exists(configFileName))
            {
                string configJson = File.ReadAllText(configFileName);
                var configContent = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(configJson);
                if (configContent != null)
                {
                    config = configContent;
                }
                else
                {
                    log.LogText($"Config file is empty: {configFileName}");
                }
                log.LogText($"Custom log folder: {config.LogDirectory}");
                log = new SmartManEncryptorLog(config.LogDirectory);
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Error occurred while reading config file: {configFileName}", ex);
            return;
        }
        try
        {
            // Delete old logs
            if (Directory.Exists(config.LogDirectory))
            {
                string[] logFiles = Directory.GetFiles(config.LogDirectory);
                foreach (string logFile in logFiles)
                {
                    FileInfo fileInfo = new FileInfo(logFile);
                    if (fileInfo.CreationTime < DateTime.Now.AddDays(-config.DeleteLogsAfterDays))
                    {
                        File.Delete(logFile);
                    }
                }
                log.LogText($"Old logs deleted.");
            }
        }
        catch (Exception ex)
        {
            log.LogError($"Error occurred while deleting old logs.", ex);
            return;
        }
        
        // Check arguments
        if (args.Length < 3)
        {
            log.LogError("Arguments not enough.");
            return;
        }
        // Check if input file exists
        string inputFilePath = args[0];
        if (!File.Exists(inputFilePath))
        {
            log.LogError($"Input file not found: {inputFilePath}");
            return;
        }
        // Check if output directory exists, if not, create it
        string outputFilePath = args[1];
        string outputDirectory = Path.GetDirectoryName(outputFilePath) ?? string.Empty;
        if (!Directory.Exists(outputDirectory) && outputDirectory is not "")
        {
            Directory.CreateDirectory(outputDirectory);
            log.LogText($"Output directory {outputDirectory} created.");
        }
        // Password protect the PDF file using PDFSharp
        string password = args[2];
        bool compress = args.Length >= 4 && args[3].ToLower() == "true";
        try
        {
            log.LogText($"Encrypting file: {inputFilePath}");
            // Get a fresh copy of the sample PDF file
            File.Copy(inputFilePath, outputFilePath, true);
            var fullFilePath = Path.GetFullPath(outputFilePath);
            log.LogText($"File copied: {fullFilePath}");
            using (PdfDocument document = PdfReader.Open(fullFilePath))
            {
                document.Options.FlateEncodeMode = PdfFlateEncodeMode.BestSpeed;
                document.Options.CompressContentStreams = compress;
                document.Options.NoCompression = !compress;
                document.SecuritySettings.UserPassword = password;
                document.SecuritySettings.OwnerPassword = password;

                //PdfSecuritySettings securitySettings = document.SecuritySettings;

                //// Setting one of the passwords automatically sets the security level to 
                //// PdfDocumentSecurityLevel.Encrypted128Bit.
                //securitySettings.UserPassword = "user";
                //securitySettings.OwnerPassword = "owner";

                //// Don't use 40 bit encryption unless needed for compatibility
                ////securitySettings.DocumentSecurityLevel = PdfDocumentSecurityLevel.Encrypted40Bit;

                //// Restrict some rights.
                //securitySettings.PermitAnnotations = false;
                //securitySettings.PermitAssembleDocument = false;
                //securitySettings.PermitExtractContent = false;
                //securitySettings.PermitFormsFill = true;
                //securitySettings.PermitFullQualityPrint = false;
                //securitySettings.PermitModifyDocument = true;
                //securitySettings.PermitPrint = false;
                log.LogText($"Saving file as {fullFilePath}");
                document.Save(fullFilePath);
            }
            log.LogText($"File encrypted: {fullFilePath}");
        }
        catch (Exception ex)
        {
            log.LogError($"Error occurred while encrypting file: {outputFilePath}", ex);
        }
    }
}
public class Config
{
    public string LogDirectory { get; set; } = "logs";
    public int DeleteLogsAfterDays { get; set; } = 90;
}