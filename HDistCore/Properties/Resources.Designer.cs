﻿//------------------------------------------------------------------------------
// <auto-generated>
//     このコードはツールによって生成されました。
//     ランタイム バージョン:4.0.30319.42000
//
//     このファイルへの変更は、以下の状況下で不正な動作の原因になったり、
//     コードが再生成されるときに損失したりします。
// </auto-generated>
//------------------------------------------------------------------------------

namespace HDistCore.Properties {
    using System;
    
    
    /// <summary>
    ///   ローカライズされた文字列などを検索するための、厳密に型指定されたリソース クラスです。
    /// </summary>
    // このクラスは StronglyTypedResourceBuilder クラスが ResGen
    // または Visual Studio のようなツールを使用して自動生成されました。
    // メンバーを追加または削除するには、.ResX ファイルを編集して、/str オプションと共に
    // ResGen を実行し直すか、または VS プロジェクトをビルドし直します。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   このクラスで使用されているキャッシュされた ResourceManager インスタンスを返します。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("HDistCore.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   すべてについて、現在のスレッドの CurrentUICulture プロパティをオーバーライドします
        ///   現在のスレッドの CurrentUICulture プロパティをオーバーライドします。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   中断しました。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatAborted {
            get {
                return ResourceManager.GetString("LogFormatAborted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0}を圧縮中 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatCompressing {
            get {
                return ResourceManager.GetString("LogFormatCompressing", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0}をコピー中 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatCopy {
            get {
                return ResourceManager.GetString("LogFormatCopy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0}を圧縮コピー中 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatCopyCompressed {
            get {
                return ResourceManager.GetString("LogFormatCopyCompressed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0}: {1} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatException {
            get {
                return ResourceManager.GetString("LogFormatException", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0}の圧縮コピーに失敗しました。通常コピーします。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatFailToExtract {
            get {
                return ResourceManager.GetString("LogFormatFailToExtract", resourceCulture);
            }
        }
        
        /// <summary>
        ///   ***終了*** に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatFinished {
            get {
                return ResourceManager.GetString("LogFormatFinished", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0} に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatNoMessage {
            get {
                return ResourceManager.GetString("LogFormatNoMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   一時停止 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatPaused {
            get {
                return ResourceManager.GetString("LogFormatPaused", resourceCulture);
            }
        }
        
        /// <summary>
        ///   再開 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatResumed {
            get {
                return ResourceManager.GetString("LogFormatResumed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   {0}の終了を待っています。 に類似しているローカライズされた文字列を検索します。
        /// </summary>
        internal static string LogFormatWaitLocked {
            get {
                return ResourceManager.GetString("LogFormatWaitLocked", resourceCulture);
            }
        }
    }
}
