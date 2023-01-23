# 自作Unityエディタ拡張まとめ
boothで配布しているものや、自分用に適当に作ったもののまとめ。
気分で更新されます。
各拡張はUPMでのインストールに対応しています。
## AddPreSuffixWindow
### このツール何？
[ここのbooth](https://meronmks.booth.pm/items/2764237)で配布している選択したGameObjectの配下全部の名前に一括でPrefix(接頭語)とSuffix(接尾語)を付与するものです。
着せ替えにおいてこのボーン何？ってのを多少ながら防止できます。
### UPMでのインストールURL
https://github.com/meronmks/meronmksTools.git?path=/Assets/meronmksTools/GameObjectsAddPreSuffix

## NewInspectorWindow
### このツール何？
[ここのbooth](https://meronmks.booth.pm/items/2277428)で配布している、単独のロック済みInspectorを表示する奴です。
Objectの右クリックメニュー等に新たにメニューが追加されてます。
### UPMでのインストールURL
https://github.com/meronmks/meronmksTools.git?path=/Assets/meronmksTools/NewInspectorWindow

## MissingSearch
### このツール何？
シーン内に存在するMissingとなった参照、スクリプトを表示します。
### UPMでのインストールURL
https://github.com/meronmks/meronmksTools.git?path=/Assets/meronmksTools/MissingSearch

## AnimationGenerator
### このツール何？
シーン上で現在設定されているシェイプキーの値をAnimationファイルとして書き出します。
書き出し対象のオブジェクトは複数指定できます。
### UPMでのインストールURL
https://github.com/meronmks/meronmksTools.git?path=/Assets/meronmksTools/AnimationGenerator

## SimpleObjectSpawn
### このツール何？
指定したオブジェクトの表示非表示をするだけの単純なアニメーションをAnimator ControllerのLayerへ追加します。
VRC想定ですがVRCSDK3が導入されていない環境でも依存しない機能については利用可能です。
VRCSDK3が導入されているとVRCExpressionsMenuおよびVRCExpressionParametersへの編集機能も有効となります。

### 注意点
オブジェクトのON/OFF切り替えを一つのアニメーションで行っているのでパーティクルなどを始めにOFFにするアニメーションの場合
一度出現してから消えることになります。

### UPMでのインストールURL
https://github.com/meronmks/meronmksTools.git?path=/Assets/meronmksTools/SimpleObjectSpawn
