# HDist
ファイルサーバーもしくはWebサーバーに置かれたファイルをSHAチェックサムを使って高速に配布するためのツールです。

まず、配布元ディレクトリのトップに`BuildSum` コマンドを使って `_checksum.sha` という隠しファイルを作成します。<br>
`_checksum.sha` には配布するファイルとそのSHAチェックサムの一覧が記載されているため、
`hcopy`/`HCopyW` はこの一覧を取得し、コピー先に既にあるファイルのチェックサムと照合して、変更されているファイルだけをコピーします。

配布元としてはファイルサーバーのディレクトリ、もしくはHTTPサーバーのURLを指定することが可能です。

## hcopy.exe / HCopyW.exe
配布元ディレクトリもしくはUriからファイルをコピーするコマンドです。<br>
`hcopy.exe` はCLIで動作するコマンド、`HCopyW.exe` はWindowsで動作するアプリケーションです。<br>
どちらもコマンドライン引数にて配布元・配布先などを指定します。

### hcopy.exe のコマンドライン引数
```
hcopy <source-uri> <destination-dir> [options]
hcopyは<source-uri>から _checksum.sha に列挙されたファイルをコピーする。

Options:
  --wait <file>
  -w <file>
      <file>の書き込みロックが解除されるまで待ってからコピーを開始する。

  --run <executable>
  -r <executable>
      コピーの終了後に指定したプログラムを起動する。

  --param <parameters>
  -p <parameters>
      --run で指定したプログラムを実行する際のパラメータを指定する。
      複数ある場合は"で括る。

  --compress <directory>
  -c <directory>
      コピー高速化のため圧縮されたファイルを格納しているディレクトリを指定する。
      <source-uri>がfile://の場合のみ有効。
      相対パスで指定した場合は<source-uri>からの相対パスになる。

  --header <header>
      <source-uri>がhttp(s) URIの場合、HTTPリクエストにヘッダを追加する。

  --header-file <file>
      <source-uri>がhttp(s) URIの場合、指定したファイルの内容をHTTPリクエストにヘッダとして追加する。

  --help
  /?
      このヘルプを表示。
```
### HCopyW.exe のコマンドライン引数
```
HCopyW <source-uri> <destination-dir> [options]
HCopyWはsource-uriから _checksum.sha に列挙されたファイルをコピーする。

Options:
  --wait <file>
  -w <file>
      <file>の書き込みロックが解除されるまで待ってからコピーを開始する

  --run <executable>
  -r <executable>
      コピーの終了後に指定したプログラムを起動する。

  --param <parameters>
  -p <parameters>
      --run で指定したプログラムを実行する際のパラメータを指定する。
      複数ある場合は"で括る。

  --compress <directory>
  -c <directory>
      コピー高速化のため圧縮されたファイルを格納しているディレクトリを指定する
      <source-uri>がfile://の場合のみ有効。
      相対パスで指定した場合は<source-uri>からの相対パスになる

  --header <header>
      <source-uri>がhttp(s) URIの場合、HTTPリクエストにヘッダを追加する。

  --header-file <file>
      <source-uri>がhttp(s) URIの場合、指定したファイルの内容をHTTPリクエストにヘッダとして追加する。

  --log <file>
  -l <file>
      ログをファイルに出力したい場合、ファイル名を指定する。
      ファイル名に日時を含みたい場合、以下のパラメータが実行時に置換される
        %Y 年(4桁)
        %M 月(2桁)
        %D 日(2桁)
        %H 時(2桁、24時間表記)
        %N 分(2桁)
        %S 秒(2桁)
        %P プロセスID

  --help
  /?
      このヘルプを表示
```
### Buildsum.exeのコマンドライン引数
```
BuildSum [<target-dir>] [options]
BuildSumは<target-dir>で指定したディレクトリに含まれるファイルの
SHA1チェックサム一覧を _checksum.sha という名前で生成する。
<target-dir>を省略した場合はカレントディレクトリが対象

Options:
  --compress <compress-dir>
  -c <compress-dir>
      圧縮ファイルを格納するディレクトリ。
      圧縮ファイルを使うことでネットワークの負荷を軽減できる。
      相対パスで指定した場合は<target-dir>からの相対パスになる。

  --ignore <name>
  -i <name>
      一覧作成時に無視するファイル名/ディレクトリ名。ワイルドカード可。
      複数のファイル名を指定したい場合、このオプションを複数回指定する。
      --ignore-listと排他的に使用する。

  --ignore-list <filename>
  -I <filename>
      一覧作成時に無視するファイル名/ディレクトリ名の一覧をファイルで指定。
      各要素は改行で区切る。ワイルドカード可。
      --ignoreと排他的に使用する。

  --include-hidden
  -h
      一覧に隠しファイルを含める。

  --help
  /?
      このヘルプを表示
```

### 圧縮コピーモード
配布元がファイルサーバーの場合、配布するファイルをbzip2形式で圧縮したファイルをコピーすることでネットワークの負荷を軽減する機能があります。
標準では配布元フォルダと同じ階層に`_cab`という隠しフォルダを作成し、そこにbzip2圧縮したファイルを配置します(`BuildSum`にて更新します)
(httpの場合はHTTP圧縮機能があり、同等のことを標準で行えるため、この機能は使用不可になっています)
