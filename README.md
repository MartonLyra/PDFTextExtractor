# PDFTextExtractor

**PDFTextExtractor** is a straightforward Windows console application designed to extract text from multiple PDF files. It was initially developed by **Niall Moran** in 2020 and has been edited by **Marton Lyra** in 2023.


To utilize the app, run it in the folder where your PDF files are located. Alternatively, you can specify the folder path as a parameter.


### Here are the available parameters::
    /f:<PDFs folder path>: Specifies the folder path where the PDF files are stored.

    /r:false or /r:true: Determines whether the application should recursively search for PDFs within subfolders. (default /r:false)

    /o:false or /o:true: Controls whether the output text file should be overwritten if it already exists. (default /o:false)
    
    /soe:false or /soe:true: Stop on Exceptions - Dictates whether the application should stop when encountering an error. If set to false, it will attempt to ignore any exceptions and continue searching for other PDF files. (detault /soe:true)

The application uses [TikaOnDotnet.TextExtractor](https://www.nuget.org/packages/TikaOnDotNet.TextExtractor/) to extract text from the PDF files and creates text files, with the same name, containing the content.


