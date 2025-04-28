環境設定須知
本專案為 ASP.NET MVC (.NET Framework 4.8) 應用程式。

由於安全考量，公開的 web.config 中未包含任何敏感資訊。
請在本地開發前，依下列需求設定環境變數或自行補齊設定。

必須設定的項目
1. AppSettings 相關


| 鍵值名稱 | 類型 | 說明 |
| :---- | :--- | :---- |
| VimeoAccessToken | string | Vimeo API 金鑰 |
| GoogleClient | string | Google OAuth Client ID |
| GoogleClientSecret | string | Google OAuth Client Secret |
| GoogleRedirectUrl | string | Google OAuth 回傳 Redirect URL |
| FrontendGoogleLogin | string | 前端使用 Google 登入後跳轉的 URL |
| VerifyAPI | string | 用於驗證的 API URL |
| FrontendResetPassword | string | 前端重設密碼頁面 URL |
| FrontendSuccessRegister | string | 前端註冊成功導向頁面 URL |
| FrontendFailRegister | string | 前端註冊失敗導向頁面 URL |

2. 資料庫連線字串 (ConnectionStrings)

鍵值名稱	說明
RecipeModel	資料庫連線字串，請設定有效的 SQL Server 連線資訊
資料庫連線字串範例：

data source=伺服器名稱;initial catalog=資料庫名稱;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework
建議設定方式
透過環境變數設定（如 Visual Studio 啟動設定 / Windows 系統環境變數）

或於本地環境的 web.config 內填入個人開發用的設定（不要推上 GitHub）

注意事項
請勿將真實的 API 金鑰、密碼、資料庫連線字串推送到公開的 GitHub 倉庫。

正式部署時，亦應透過安全方式注入這些資訊（如 IIS 應用程式設定或 CI/CD Secrets）。
