# BDO-LanguageData-Tool

Công cụ dùng để giải nén/nén, chuyển đổi định dạng tệp về dạng TSV để chỉnh sửa ngôn ngữ trong game Black Desert Online.

Yêu cầu: [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) hoặc bất kỳ thư viện Zlib tương tự.

```
PM> Install-Package SharpZipLib -Version 1.2.0
```

Cách sử dụng:

```
Kéo tệp language data của game (bản tiếng Anh là languagedata_en.loc) vào tool để tạo tệp TSV có thể chỉnh sửa.
```

```
Kéo tệp TSV đã chỉnh sửa vào tool để tạo tệp .loc copy vào thư mục game (ads/languagedata_en.loc).
```
