# VPM パッケージテンプレート

パッケージを作成するためのスターター、それらを構築して公開するための自動化など。

すべてセットアップすると、このリポジトリに変更をプッシュし、.zip および.unityPackage バージョンが自動的に生成され、このパッケージの更新を提供するために VPM で機能するリストが作成されます。さまざまなパッケージを使用してリストを作成する場合は、[テンプレートパッケージリスト](https://github.com/vrchat-community/template-package-listing)のリポジトリをチェックしてください。

## ▶ 開始

- [![このテンプレートを使用](https://user-images.githubusercontent.com/737888/185467681-e5fdb099-d99f-454b-8d9e-0760e5a6e588.png)](https://github.com/vrchat-community/template-package/generate)を押して、このテンプレートに基づいて新しい GitHub プロジェクトを開始します。
- フィッティングリポジトリの名前と説明を選択します。
- 「パブリック」に可視性を設定します。「プライベート」を選択して、後で変更することもできます。
- 「すべてのブランチを含める」を選択する必要はありません。
  -Git を使用してこのリポジトリをローカルにクローンします。
- Git と Github に不慣れな場合は、[Github のドキュメント](https://docs.github.com/en/get-started/quickstart/git-and-github-learning-resources)をご覧ください。
- フォルダーを Unity ハブに追加し、Unity プロジェクトとして開きます。
- プロジェクトを開いた後、VPM リゾルバーがダウンロードされ、プロジェクトに追加されたら待ちます。
- これにより、VPM パッケージメーカーとパッケージリゾルバーツールにアクセスできます。

## 🚇 アセットからパッケージへの移行

[アセットを VPM パッケージに変換](https://vcc.docs.vrchat.com/guides/convert-unitypackage)を参照。

## ✏️ あなたのパッケージで作業

- Delete the "Packages/com.vrchat.demo-template" directory or reuse it for your own package.
  - If you reuse the package, don't forget to rename it and add generated meta files to your repository!
- Update the `.gitignore` file in the "Packages" directory to include your package.
  - For example, change `!com.vrchat.demo-template` to `!com.username.package-name`.
  - `.gitignore` files normally _exclude_ the contents of your "Packages" directory. This `.gitignore` in this template show how to _include_ the demo package. You can easily change this out for your own package name.
- Open the Unity project and work on your package's files in your favorite code editor.
- When you're ready, commit and push your changes.
- Once you've set up the automation as described below, you can easily publish new versions.

## 🤖 Setting up the Automation

以下で説明する名前と値を持つリポジトリ変数を作成します。
リポジトリ変数の作成方法の詳細については、[リポジトリの構成変数の作成](https://docs.github.com/en/actions/learn-github-actions/variables#creating-configuration-variables-for-a-repository)を参照してください。
**リポジトリ秘密**ではなく、**リポジトリ変数**を作成していることを確認してください。

- `PACKAGE_NAME`: the name of your package, like `com.vrchat.demo-template`.

Finally, go to the "Settings" page for your repo, then choose "Pages", and look for the heading "Build and deployment". Change the "Source" dropdown from "Deploy from a branch" to "GitHub Actions".

That's it!
Some other notes:

- We highly recommend you keep the existing folder structure of this template.
  - The root of the project should be a Unity project.
  - Your packages should be in the "Packages" directory.
  - If you deviate from this folder structure, you'll need to update the paths that assume your package is in the "Packages" directory on lines 24, 38, 41 and 57.
- If you want to store and generate your web files in a folder other than "Website" in the root, you can change the `listPublicDirectory` item [here in build-listing.yml](.github/workflows/build-listing.yml#L17).

## 🎉 Publishing a Release

You can make a release by running the [Build Release](.github/workflows/release.yml) action. The version specified in your `package.json` file will be used to define the version of the release.

## 📃 Rebuilding the Listing

Whenever you make a change to a release - manually publishing it, or manually creating, editing or deleting a release, the [Build Repo Listing](.github/workflows/build-listing.yml) action will make a new index of all the releases available, and publish them as a website hosted fore free on [GitHub Pages](https://pages.github.com/). This listing can be used by the VPM to keep your package up to date, and the generated index page can serve as a simple landing page with info for your package. The URL for your package will be in the format `https://username.github.io/repo-name`.

## 🏠 Customizing the Landing Page (Optional)

The action which rebuilds the listing also publishes a landing page. The source for this page is in `Website/index.html`. The automation system uses [Scriban](https://github.com/scriban/scriban) to fill in the objects like `{{ this }}` with information from the latest release's manifest, so it will stay up-to-date with the name, id and description that you provide there. You are welcome to modify this page however you want - just use the existing `{{ template.objects }}` to fill in that info wherever you like. The entire contents of your "Website" folder are published to your GitHub Page each time.

## 💻 Technical Stuff

You are welcome to make your own changes to the automation process to make it fit your needs, and you can create Pull Requests if you have some changes you think we should adopt. Here's some more info on the included automation:

### Build Release Action

[release.yml](/.github/workflows/release.yml)

This is a composite action combining a variety of existing GitHub Actions and some shell commands to create both a .zip of your Package and a .unitypackage. It creates a release which is named for the `version` in the `package.json` file found in your target Package, and publishes the zip, the unitypackage and the package.json file to this release.

### Build Repo Listing

[build-listing.yml](.github/workflows/build-listing.yml)

This is a composite action which builds a vpm-compatible [Repo Listing](https://vcc.docs.vrchat.com/vpm/repos) based on the releases you've created. In order to find all your releases and combine them into a listing, it checks out [another repository](https://github.com/vrchat-community/package-list-action) which has a [Nuke](https://nuke.build/) project which includes the VPM core lib to have access to its types and methods. This project will be expanded to include more functionality in the future - for now, the action just calls its `BuildRepoListing` target.
