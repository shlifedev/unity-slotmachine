:: 엑셀파일이 존재하는 경로 입력하세요
:: ex) c:\....\excels

rem excelPath
set excelPath=xlsx

:: cs파일을 저장할 경로
:: ex) c:\....\cs
rem csPath
set csPath=..\..\..\Assets\Scripts\Table\

:: json파일을 저장할 경로
:: ex) c:\....\json
rem jsonPath
set jsonPath=..\..\..\Assets\Resources\TableDatas\

TB.exe %excelPath% %csPath% %jsonPath%

cmd