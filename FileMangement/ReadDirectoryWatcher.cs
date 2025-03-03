using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static FileClassifier.FileWatchController;

namespace FileClassifier
{
    /// <summary>
    /// ReadDirectoryChangesW DLLを使用するためのクラス。
    /// </summary>
    internal class ReadDirectoryWatcher : FileWatcher
    {
        #region DLLの宣言

        /// <summary>
        /// ファイルを監視するためのメソッド。
        /// </summary>
        /// <param name="hDirectory">監視するディレクトリのハンドル</param>
        /// <param name="lpBuffer">監視結果を格納するためのバッファへのポインタ。バッファのサイズに応じた構造体が格納されます。</param>
        /// <param name="nBufferLength">バッファのサイズ（バイト単位）を指定します。</param>
        /// <param name="bWatchSubtree">サブディレクトリも監視するかどうかを指定するブール値。trueを指定すると、サブディレクトリも監視します。</param>
        /// <param name="dwNotifyFilter">監視に使うフィルタ条件</param>
        /// <param name="lpBytesReturned">監視結果バッファに格納されたデータのサイズを受け取るためのポインタ。</param>
        /// <param name="lpOverlapped">非同期 I/O 操作に必要な構造体へのポインタ。タスク処理だといらんみたい</param>
        /// <param name="lpCompletionRoutine">完了ルーチンへのポインタ</param>
        /// <returns>真なら処理が成功した、つまりエラーがないということ</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool ReadDirectoryChangesW(
            IntPtr hDirectory,
            IntPtr lpBuffer,
            uint nBufferLength,
            bool bWatchSubtree,
            uint dwNotifyFilter,
            out uint lpBytesReturned,
            IntPtr lpOverlapped,
            IntPtr lpCompletionRoutine);

        /// <summary>
        /// 指定されたファイルまたはディレクトリのハンドルを開くか、作成するメソッド。
        /// </summary>
        /// <param name="lpFileName">オープンまたは作成するファイルまたはディレクトリのパスを指定します。</param>
        /// <param name="dwDesiredAccess">要求するアクセス権を指定します。例として、FileAccess.Read（読み取り専用）やFileAccess.Write（書き込み専用）などがあります。</param>
        /// <param name="dwShareMode">他のプロセスがこのファイルまたはディレクトリに対してどのようにアクセスできるかを指定します。例として、FileShare.None（他のプロセスのアクセスを禁止）やFileShare.ReadWrite（読み書きを許可）などがあります。</param>
        /// <param name="lpSecurityAttributes">セキュリティ属性を指定します。通常はIntPtr.Zeroを指定しますが、特定のセキュリティ設定が必要な場合に使用します。</param>
        /// <param name="dwCreationDisposition">ファイルまたはディレクトリの作成方法を指定します。例として、FileMode.Open（既存のファイルをオープン）やFileMode.Create（新しいファイルを作成）などがあります。</param>
        /// <param name="dwFlagsAndAttributes">ファイルまたはディレクトリの属性やフラグを指定します。例として、FileAttributes.Normal（通常の属性）やFileAttributes.Directory（ディレクトリを示す）などがあります。</param>
        /// <param name="hTemplateFile">新しいファイルの作成時に使用するテンプレートファイルのハンドルを指定します。通常はIntPtr.Zeroを指定します。</param>
        /// <returns>成功した場合は指定したファイルまたはディレクトリのハンドルを返し、失敗した場合はIntPtr.Zeroを返します。</returns>
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            FileAccess dwDesiredAccess,
            FileShare dwShareMode,
            IntPtr lpSecurityAttributes,
            FileMode dwCreationDisposition,
            FileAttributes dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        /// <summary>
        /// 指定されたハンドルを閉じるメソッド。リソースリークを防ぎ、システムリソースを解放します。
        /// </summary>
        /// <param name="hHandle">閉じるハンドルを指定します。通常はCreateFileメソッドで取得したハンドルを指定します。</param>
        /// <returns>成功した場合は trueを返し、失敗した場合は falseを返します。失敗した場合、エラーコードはMarshal.GetLastWin32Error()で取得できます。</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hHandle);


        #endregion DLLの宣言

        #region DLLが使用する構造体の宣言

        /// <summary>
        /// ReadDirectoryChangesW の監視処理の結果を受け取るための構造体。
        /// C# 側に存在しないから用意してると思うんだけど、もう少し詳しく調べようね。
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct FILE_NOTIFY_INFORMATION
        {
            public uint NextEntryOffset; // 次のエントリのオフセット（4バイト）
            public uint Action;           // アクションの種類（4バイト）
            public uint FileNameLength;   // ファイル名の長さ（4バイト）
            public char FileName;         // ファイル名（可変長）

            // ファイル名が20文字の場合40バイト

            // 以上のメモリの長さを意識してバッファを作ろう
        }

        #endregion

        #region 定数の宣言。

        /// <summary>
        /// ReadDirectoryChangesW で使用するフィルター。
        /// ファイル名の変更を監視。ファイルの追加、削除、名前変更が含まれます。
        /// 使うのはこれだね
        /// </summary>
        private const uint FILE_NOTIFY_CHANGE_FILE_NAME = 0x00000001;

        /// <summary>
        /// ReadDirectoryChangesW で使用するフィルター。
        /// ディレクトリ名の変更を監視します。ディレクトリの追加、削除、名前変更が含まれます。
        /// フォルダ追加を見れるようだね
        /// </summary>
        private const uint FILE_NOTIFY_CHANGE_DIR_NAME = 0x00000002;

        /// <summary>
        /// ReadDirectoryChangesW で使用するフィルター。
        /// ファイルまたはディレクトリの作成日時の変更を監視します。新しいファイルやディレクトリが作成された場合に発生します。
        /// こいつ大本命だな。
        /// </summary>
        private const uint FILE_NOTIFY_CHANGE_CREATION = 0x00000040;

        #region 多分使わん

        /// <summary>
        /// ReadDirectoryChangesW で使用するフィルター。
        /// ファイルやディレクトリの属性の変更を監視します。たとえば、読み取り専用や隠し属性の変更が含まれます。
        /// </summary>
        private const uint FILE_NOTIFY_CHANGE_ATTRIBUTES = 0x00000004;

        /// <summary>
        /// ReadDirectoryChangesW で使用するフィルター。
        /// ファイルサイズの変更を監視します。ファイルの内容が変更された場合に発生します。
        /// </summary>
        private const uint FILE_NOTIFY_CHANGE_SIZE = 0x00000008;

        /// <summary>
        /// ReadDirectoryChangesW で使用するフィルター。
        /// 最終書き込み日時の変更を監視します。ファイルが最後に書き込まれた日時が変更された場合に発生します。
        /// </summary>
        private const uint FILE_NOTIFY_CHANGE_LAST_WRITE = 0x00000010;

        /// <summary>
        /// ReadDirectoryChangesW で使用するフィルター。
        /// 最終アクセス日時の変更を監視します。ファイルが最後にアクセスされた日時が変更された場合に発生します。
        /// </summary>
        private const uint FILE_NOTIFY_CHANGE_LAST_ACCESS = 0x00000020;

        /// <summary>
        /// ReadDirectoryChangesW で使用するフィルター。
        /// セキュリティ情報の変更を監視します。ファイルやディレクトリのアクセス権が変更された場合に発生します。
        /// </summary>
        private const uint FILE_NOTIFY_CHANGE_SECURITY = 0x00000100;

        #endregion

        /// <summary>
        /// 監視結果がファイル追加であるかを確認するための定数。
        /// こいつだとダウンロード中の tempファイルにも反応しちゃうな
        /// </summary>
        private const uint FILE_ACTION_ADDED = 0x00000001;

        /// <summary>
        /// 監視結果がファイルの変更であるかを確認するための定数。<br/>
        /// 追加されたファイルは大体最初は .temp ファイルなので、ダウンロードが完了してちゃんとしたファイルになったらこれが呼ばれる。
        /// </summary>
        private const uint FILE_ACTION_MODIFIED = 0x00000003;

        /// <summary>
        /// 監視結果がファイルの名前変更であるかを確認するための定数。<br/>
        /// .jpg.crdownload ファイルみたいな一時ファイルでダウンロードされて、ダウンロードが完了してちゃんとしたファイルになったらこれが呼ばれる。
        /// </summary>
        private const uint FILE_ACTION_RENAMED_NEW_NAME = 0x00000005;

        /// <summary>
        /// 結果受け取り用のバッファのサイズ。
        /// </summary>
        private const int BufferSize = 1024 * 64; // 64KBのバッファ。 byteは一要素1バイトだから単純に64*1024倍

        #endregion

        #region フィールド

        /// <summary>
        /// 監視のために開いているファイルのディレクトリ。
        /// </summary>
        private IntPtr watchDirectory;

        /// <summary>
        /// 監視結果を取得するためのバッファ。
        /// </summary>
        private readonly byte[] resultBuffer;

        /// <summary>
        /// タスクキャンセル用のトークン。
        /// </summary>
        private CancellationTokenSource cancellationTokenSource;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// データを受け取り、さらにディレクトリの監視を開始する
        /// </summary>
        /// <param name="watchPath">監視先</param>
        /// <param name="controller">報告先</param>
        /// <exception cref="IOException"></exception>
        public ReadDirectoryWatcher(string watchPath, FileWatchController controller) : base(watchPath, controller)
        {
            // 監視のためにファイルを開く
            watchDirectory = CreateFile(
                watchPath,
                FileAccess.Read, // 読み取り専用でオープン
                FileShare.ReadWrite | FileShare.Delete,  // 他のプロセスからのアクセスを許可する。読み書き削除自由
                IntPtr.Zero,
                FileMode.Open,   // 既存のディレクトリをオープン
                (System.IO.FileAttributes)0x02000000, // 通常の属性を指定
                IntPtr.Zero);

            // ハンドルが無効な場合のエラーチェック
            if ( watchDirectory == IntPtr.Zero )
            {
                throw new IOException("ディレクトリを開けませんでした。");
            }

            // 監視結果受け取り用のバッファのメモリ確保
            resultBuffer = new byte[BufferSize]; // バッファの初期化

            // 非同期監視開始。
            cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => Watch(cancellationTokenSource.Token), cancellationTokenSource.Token);
        }

        #endregion

        #region Publicメソッド。

        /// <summary>
        /// 監視処理を停止してインスタンスを破棄する。
        /// スレッドを追加していればそちらも閉じる
        /// </summary>
        public async override void Dispose()
        {
            // キャンセルトークンを発動させて処理を止める。
            cancellationTokenSource.Cancel();

            await Task.Delay(100); // 擬似的な処理

            //　監視先パスがあるならファイルの読み込みをやめる。
            if ( watchDirectory != IntPtr.Zero )
            {
                if ( !CloseHandle(watchDirectory) )
                {
                    int error = Marshal.GetLastWin32Error();
                    // エラーログを出力するか、適切なエラーハンドリングを行う
                    Console.WriteLine($"CloseHandle failed with error code: {error}");
                }
                watchDirectory = IntPtr.Zero;
            }
        }

        #endregion

        #region privateメソッド

        /// <summary>
        /// 監視処理の実体。
        /// Taskとして処理する。
        /// </summary>
        /// <param name="cancellationToken"></param>
        private void Watch(CancellationToken cancellationToken)
        {
            // バッファの開始地点を記録。
            IntPtr bfStart = Marshal.UnsafeAddrOfPinnedArrayElement(resultBuffer, 0);

            // キャンセルされるまで処理を続ける。
            while ( !cancellationToken.IsCancellationRequested )
            {
                // 監視処理の実装
                // ここでReadDirectoryChangesWを呼び出す
                uint bytesReturned;// 結果情報の長さ。何バイトかな
                bool success = ReadDirectoryChangesW(
                    watchDirectory,
                    bfStart,// バッファの開始ポインタ
                    (uint)resultBuffer.Length,
                    false,// このディレクトリの子フォルダは監視しない。
                    FILE_NOTIFY_CHANGE_CREATION | FILE_NOTIFY_CHANGE_FILE_NAME, // 新規ファイル作成のみ検知
                    out bytesReturned,
                    IntPtr.Zero,
                    IntPtr.Zero);


                // 処理が成功した場合は監視結果を読み取り。
                // ここでの成功とは変更が確認できたことではなく、監視処理がつつがなく、エラーなく終わったということ。
                if ( success )
                {
                    // 結果を受け取るバッファからデータを読むための準備
                    // この結果バッファのポインタの始点読み込みは毎回二回ずつやる意味あるか？　最初に取得して使いまわせそう。
                    IntPtr ptr = bfStart;

                    // 構造体内のファイル名の位置を取得
                    int fileNameOffset = (int)Marshal.OffsetOf(typeof(FILE_NOTIFY_INFORMATION), "FileName");

                    // バッファのどこまでデータを読んだか、の記録用
                    int offset = 0;

                    // 結果情報を全て読み終えるまでループ
                    while ( offset < bytesReturned )
                    {

                        // 監視結果情報を取得
                        var notifyInfo = (FILE_NOTIFY_INFORMATION)Marshal.PtrToStructure(ptr, typeof(FILE_NOTIFY_INFORMATION));

                        // 全キャンセルフラグ
                        bool isCancel = false;

                        // 変更の種類が修正（作成フィルタ＆修正フィルタ）ならコントローラーに通知する
                        if ( notifyInfo.Action == FILE_ACTION_MODIFIED )
                        {
                            // 変更を通知。
                            isCancel = ReportNewFile(ReturnFilePath(ptr, fileNameOffset, (int)notifyInfo.FileNameLength));
                        }
                        // 追加の場合は作成されたファイルなのかをチェックしてから報告
                        else if ( notifyInfo.Action == FILE_ACTION_ADDED || notifyInfo.Action == FILE_ACTION_RENAMED_NEW_NAME )
                        {
                            string filePath = ReturnFilePath(ptr, fileNameOffset, (int)notifyInfo.FileNameLength);

                            // 新規作成ファイルなら報告
                            if ( isNewFile(filePath) )
                            {
                                isCancel = ReportNewFile(filePath);
                            }
                        }

                        //string fPath = ReturnFilePath(ptr, fileNameOffset, (int)notifyInfo.FileNameLength);

                        // 次のエントリがない場合、あるいは全キャンセルならループを終了
                        if ( notifyInfo.NextEntryOffset == 0 || isCancel )
                        {
                            break;

                        }
                        // 次のエントリへのポインタ移動
                        else
                        {
                            // 結果情報バッファ内の次の結果のポインタに進む。
                            offset += (int)notifyInfo.NextEntryOffset;
                            ptr = IntPtr.Add(ptr, (int)notifyInfo.NextEntryOffset);
                        }

                    }
                }

#if DEBUG
                else
                {
                    // デバッグ実行中のみエラー原因を特定。
                    int errorCode = Marshal.GetLastWin32Error();
                    // エラー処理: 例外をスローするなど
                    throw new IOException("ReadDirectoryChangesW failed.", errorCode);
                }
#endif
            }
        }

        /// <summary>
        /// 追加ファイルが今作成されて追加されたのかを確認する。
        /// </summary>
        /// <param name="notifyInfo"></param>
        /// <returns></returns>
        private bool isNewFile(string fullPath)
        {
            // 一時ファイル、存在しないパスは戻す。
            if ( ClassifyManager.tempExcludeHash.Contains(Path.GetExtension(fullPath)) || !File.Exists(fullPath) )
            {
                return false;
            }

            var fileInfo = new FileInfo(fullPath);

            // 作成日時と更新日時が一致する(差が十秒以下)ならそれは出力ファイル
            if ( (fileInfo.LastAccessTime - fileInfo.CreationTime).TotalSeconds < 10 )
            {
                return true; // 新規ファイルと見なす
            }

            // その他の条件に基づく処理
            return false; // 移動されたファイルと見なす
        }

        /// <summary>
        /// ファイル監視結果からファイルパスを取得するメソッド。
        /// </summary>
        /// <param name="ptr">監視結果の構造体のポインタ</param>
        /// <param name="nameOffset">名前フィールドの位置</param>
        /// <param name="length">名前の長さ</param>
        /// <returns>ファイルのフルパス</returns>
        private string ReturnFilePath(IntPtr ptr, int nameOffset, int length)
        {
            // ファイル名のポインタを計算
            // 可変長ですから、構造体の最後にメモリが確保されてる。
            // 構造体のポインタにFileNameの開始位置を足してやる。
            IntPtr fileNamePtr = IntPtr.Add(ptr, (int)nameOffset);

            // 変更されたファイル名を取得し、監視ディレクトリと繋げてフルパスにする。
            // ファイル名はパス抜きだからね
            // UTF-16なので割る2
            return Path.Combine(watchPath, Marshal.PtrToStringUni(fileNamePtr, length >> 1));// 割る2はビットシフトでやると速いらしい。

        }


        #endregion

    }
}
