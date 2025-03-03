using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Forms;

namespace FileClassifier
{

    /// <summary>
    /// ファイル分類で必要なもの
    /// ・ファイル分類設定データの定義。
    /// ・データの保存、読み込み。
    /// ・ファイル監視設定。
    /// ・分類設定変更画面。
    /// ・全体設定画面。
    /// ・分類実行画面。
    /// ・エクスプローラー上での分類実行。
    /// 
    /// 自動監視するか、とか監視対象拡張子とかの設定は全体設定画面でできるようにするか。
    /// 設定データ共々ここで読み取る。
    /// 
    /// 監視して画面を出すことはできた。
    /// 後葉ドロップボックスで使用する分類データを選ぶだけ。
    /// その分類データで置き換え文字を入力する設定ならテキストボックスを出す。
    /// 保存する、分類データを選ぶ、の2ステップで行ける
    /// 
    /// エクスプローラーで不要ファイルを同フォルダ内で「廃棄フォルダ」に移動させる機能を。あと、エクスプローラーで選択したファイルを任意に分類する機能を
    /// 
    /// </summary>
    static class Program
    {

        /// <summary>
        /// 設定画面のインスタンス。<br/>
        /// 複数インスタンス生成されるのを防ぐ。<br/>
        /// </summary>
        public static SettingForm setting;

        /// <summary>
        /// タスクトレイ管理
        /// </summary>
        private static NotifyIcon notifyIcon;

        [STAThread]
        static void Main()
        {
            // アプリが新規に立ち上がっているかの確認フラグ。
            bool createdNew;
            using ( Mutex mutex = new Mutex(true, ClassifyManager.appName, out createdNew) )
            {
                // 重複実行の場合戻る。
                if ( !createdNew )
                {
                    MessageBox.Show("すでにアプリは実行されています。", "警告", buttons: MessageBoxButtons.OK);
                    Application.Exit();// 既に実行中の場合は終了
                    return;
                }
            }

            // 未処理のスレッド例外を捕捉
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);

            // 未処理の非スレッド例外を捕捉
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // アプリケーション終了時の処理
            Application.ApplicationExit += new EventHandler(OnApplicationExit); // イベントハンドラ登録

            // データのロード
            ClassifyManager.LoadData();

            // Windows フォームアプリケーションの初期化
            // フォントやUIの設定。
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // スタートアップに登録
            RegisterStartup();

            // FileSystemWatcher を開始
            StartWatching();

            // システムトレイに表示。
            SetSystemTray();

            // メインループを開始（フォームは表示しない）
            // バックグラウンドで動く。
            Application.Run();


        }


        #region アプリ立ち上げ処理

        /// <summary>
        /// レジストリにアプリを登録してスタートアップアプリにするメソッド。
        /// </summary>
        private static void RegisterStartup()
        {
            string appPath = Application.ExecutablePath;

            using ( var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(
                "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true) )
            {
                key.SetValue(ClassifyManager.appName, appPath);
            }
        }

        /// <summary>
        /// 監視開始メソッド。
        /// </summary>
        private static void StartWatching()
        {
            // 最初にダウンロードパスの設定をする。
            if ( ClassifyManager.appSetting.isWatchDL )
            {
                ClassifyManager.watchController.AddPath($"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\Downloads");
            }

            // 追加パスを追加
            // アプリ設定からも変更できるようにせんとな
            for ( int i = 0; i < ClassifyManager.appSetting.exWatchPath.Count; i++ )
            {
                ClassifyManager.watchController.AddPath(ClassifyManager.appSetting.exWatchPath[i]);
            }
        }

        #endregion

        #region システムトレイ関連処理

        /// <summary>
        /// システムトレイ（右下の小さいアイコン）にアプリの処理を表示できるようにする。<br/>
        /// ここを押すと設定画面が出るように。
        /// </summary>
        private static void SetSystemTray()
        {
            // すでにあるなら戻る
            if ( notifyIcon != null )
            {
                return;
            }

            // NotifyIconの設定
            // このインスタンスはシステムトレイにアイテムを表示する機能のラッパー。使う時だけインスタンス化で良い。
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon(ClassifyManager.iconPath),
                Visible = true,

                // 設定ウィンドウを開く処理を仕込む。
                ContextMenu = new ContextMenu(new[]
                {
                    // 設定
                    new MenuItem("設定", ShowSettings),
                    // アプリ終了
                    new MenuItem("アプリ終了",(s, e) => Application.Exit())
                })
            };
        }

        /// <summary>
        /// 設定ウィンドウを表示するメソッド
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ShowSettings(object sender, EventArgs e)
        {

            // すでにウィンドウが開いている場合は何もしない
            if ( setting != null )
            {
                setting.Activate();
                return;
            }

            // 新しいウィンドウを作成し、表示する
            setting = new SettingForm();
            setting.FormClosed += (s, args) => setting = null; // ウィンドウが閉じられたときにnullに設定
            setting.Show();
        }

        #endregion システムトレイ関連処理

        #region 例外処理

        /// <summary>
        /// UIスレッドで発生した未処理例外を処理する。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        /// <summary>
        /// 非UIスレッドで発生した未処理例外を処理する。
        /// </summary>
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if ( e.ExceptionObject is Exception exception )
            {
                HandleException(exception);
            }
        }

        /// <summary>
        /// まとめて例外処理
        /// </summary>
        /// <param name="ex"></param>
        private static void HandleException(Exception ex)
        {
            if ( ex is FileNotFoundException )
            {
                MessageBox.Show($"エラー: 移動元のファイルが見つかりません。詳細: {ex.Message}", "ファイル未発見", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if ( ex is DirectoryNotFoundException )
            {
                MessageBox.Show($"エラー: 移動先のディレクトリが見つかりません。詳細: {ex.Message}", "ディレクトリ未発見", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if ( ex is IOException )
            {
                MessageBox.Show($"エラー: 入出力エラーが発生しました。詳細: {ex.Message}", "入出力エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if ( ex is UnauthorizedAccessException )
            {
                MessageBox.Show($"エラー: アクセスが拒否されました。詳細: {ex.Message}", "アクセス拒否", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show($"エラー: 不明なエラーが発生しました。詳細: {ex.Message}", "不明なエラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // アプリ終了。
            Application.Exit();
        }
        #endregion 例外処理

        /// <summary>
        /// アプリケーション終了時の処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnApplicationExit(object sender, EventArgs e)
        {

            notifyIcon.Visible = false;
            notifyIcon.Dispose();

            // 全監視オブジェクトを破棄
            ClassifyManager.watchController.Dispose();

            // 全部やめる
            Environment.Exit(0);
        }
    }

}
