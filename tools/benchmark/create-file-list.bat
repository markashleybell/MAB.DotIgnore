forfiles /p C:\Src\eShopOnWeb /s /c "cmd /c if @isdir==FALSE echo @path" >>filelist.txt