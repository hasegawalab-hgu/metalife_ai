# Gather_Lab（仮称）
更新履歴  
2025年 2月4日  
  

## 使用ツール
- Unity 2022.3.24
- Photon Fusion2 （無料プラン）
- PlayFab （無料プラン）
- ChatGPT-4o

## このツールについて
このツールはオフラインプレイヤーをAIで制御して活性化させたバーチャルオフィスである。  
参考にしたバーチャルオフィスのサイトは[Gather.io](https://ja.gather.town/)と[MetaLife](https://metalife.co.jp/)で、ログインやチャット、キャラクターの移動、変更など基本的な機能を実装してある。  
行動生成はステート遷移をするルールベースで制御している。  
対話応答の生成はChatGPT-4oのAPI機能で実装しており、DMの内容（jsonまんま）+ プロンプト + 送信した内容を全てChatGPTに送ることで実装している。  
  
このプロジェクトは少し規模が大きめなので絶対に[unity拡張機能を導入したvscode](https://zenn.dev/iwatos/articles/6a19af30e4cad7)とgitを使用することをおすすめする。ちなみにgitに関してはgit-lfsというツールも使用して大きめのデータも保存できるようにしている。  

## 注意点
- ChatGPTのAPIキーは現在デバイスに保存する方法をとっているため、別のPCから同じアカウントへログインする際は再度APIキーを入力する必要がある
- ChatGPTに関わらず発行したAPIキーはgithubにpushしないこと（privateリポジトリや）




## プロジェクトファイルのダウンロード方法（macの場合）
  
### [homebrewインストール](https://brew.sh/ja/)
### git インストール
```
brew install git
```
### git-lfs インストール
```
brew install git-lfs
```
### プロジェクト


