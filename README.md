# sm-pdf-encryptor

**sm-pdf-encryptor** is an open-source executable file designed to add password protection to a PDF file through the command-line interface. It also provides the option for file compression. Below are the details of the program.

## Program Details

- **Program Name:** `sm-pdf-encrypt.exe`
- **Program Purpose:** Add password protection and optional compression to PDF files.

## Command Format

```bash
sm-pdf-encrypt.exe -i input_file_path -o output_file_path -p password [-c] [-l log_folder] [-d days_to_keep_logs] [-r] [-v]
```
## Parameters Explanation

-i: Input file path (required)
-o: Output file path (required)
-p: Password (required)
-c: Enable zip compression
-l: Log file output folder (default: logs)
-d: Number of days to keep log files (default: 90)
-r: Remove the original input file
-v: Record detailed information to log

## Examples
## Simple Example:

```bash
sm-pdf-encrypt.exe -i my_file.pdf -o my_encrypted_file.pdf -p A123456789
```

This example protects the file my_file.pdf with the password A123456789 and generates a new file named my_encrypted_file.pdf.
Complex Example:

```bash
sm-pdf-encrypt.exe -i "C:/Users/Frank Huang/Desktop/SmartManEncryptor/0000MAXE.pdf" -o ./output_folder/0000MAXE_encrypted.pdf -c -d 30 -l "C:/Users/Frank Huang/Desktop/my_log_folder" -p 12345 -v -r
```
This example encrypts 0000MAXE.pdf, compresses the file, and outputs it to ./output_folder/0000MAXE_encrypted.pdf. It uses the password 12345, records detailed information in logs, and deletes logs older than 30 days. The resulting log is stored in C:/Users/Frank Huang/Desktop/my_log_folder. The original input file is deleted.

## Troubleshooting
- 	Invalid value for -d option (should be a number)  
	Provide a positive integer for the number of days to keep log files.
-	Invalid argument (Should be -i, -o, -p, -c, -l, -d, or -r)  
	Ensure that the provided arguments match the valid options.
-	Error occurred while parsing command-line arguments.  
	Refer to the error message or notify the developer for unexpected errors during parameter parsing.
-	Invalid arguments. Some required arguments are missing.  
	Ensure all required parameters are provided.
-	Input file does not exist  
	The specified input file does not exist.
-	Error occurred while deleting old logs.  
	Unexpected error while deleting old log files. Refer to the error message or notify the developer.
-	Error occurred while encrypting PDF.  
	Error during PDF encryption. Refer to the error message or notify the developer.
-	Error occurred while compressing the file  
	Error during file compression. Refer to the error message or notify the developer.
-	Error occurred while removing the original file.  
	Error during the removal of the original file. Refer to the error message or notify the developer.

