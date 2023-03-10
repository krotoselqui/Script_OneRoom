## 燃える薪

#### 作成日時
- 2022年11月21日 ～ (2023年3月10日)

#### 概要
- 薪を持つことができ、
指定した場所に置くことにより、燃やす体験ができる.

#### 作成目的
- ワールド『ぽかぽかの冬』において、薪をくべられる体験の再現
- 参考:![簡易仕様書](/bonfire firewood/Image/ref_maki.png) 

#### 定義
- 薪 : 当スクリプトを、UdonBehaviourとしてAsset経由でアタッチしたGameObject.
- 燃える : 薪を手放した際に、薪から音とパーティクルが発生する.

#### 設定項目
- `burnSound` : 燃焼音.この配列の中のうち1つだけ指定され、再生される.
- `audioSrc` : `burnSound`を再生するAudioSource.
- `fireTransform` : 置くと薪の燃える位置.この位置のうち最も近い位置までの距離をもとに、燃えるかどうかが判定される.
- `distBurnThreshold` : `fireTransform`のうち最も近い位置で判定する際に、燃えたとみなす距離(m).
- `burnParticle` : 燃える際に再生が行われるパーティクル.すべて再生される.
- `respawnTime` : 薪が**燃え始めてから**、元の位置に戻るまでの時間(秒).

#### 留意点
- `burnParticle`には停止処理を行っていない. 永続するパーティクルは発生し続ける可能性がある.
- `respawnTime`は、本来音声とパーティクルの長さを考慮し、燃焼後のタイミングから指定すべきものであるが、音声が複数存在すること、パーティクルは発生時間と見た目とで差を生じることなどから、手を離した瞬間を開始時間としている.燃焼時間を含むことを注意されたい.

#### 仕様
- 燃えるかどうかの判定は手放し`OnDrop`のタイミングで呼ばれる.
- いずれの音声も、あらかじめAudioSourceに設定された、同一の音量で再生される.

#### 想定される設定ミス/今後発生しうる問題点
- `distBurnThreshold`の設定値を過大に設定し、意図しない距離で燃える
- `VRCPickUp`の挙動に変更が加わる.

#### 流用可能スニペット
```csharp
    //VRCPICKUP
    [FieldChangeCallback(nameof(vrcPick))]
    private VRC_Pickup _vrcPick;
    private VRC_Pickup vrcPick => _vrcPick ? _vrcPick : (_vrcPick = (VRC_Pickup)GetComponent(typeof(VRC_Pickup)));

    //DETACHABLE WITH VRCPICKUP DECLARES
    private void DisablePickUp() => vrcPick.pickupable = false;
    private void EnablePickUp() => vrcPick.pickupable = true;
    private void PlayHaptics() => vrcPick.PlayHaptics();
    private void SetTextPickup(string st) => vrcPick.InteractionText = st;
    private void SetTheftablePickup(bool b) => vrcPick.DisallowTheft = b;
```

