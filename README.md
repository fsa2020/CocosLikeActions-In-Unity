# CocosLikeActions In Unity By C#
This is a group of  simple 3d animations similar with cocos2dx actions ，you can build animation only in a few lines of code .
Including  action sequence,merge and ease.

# Code Example

Simple Move
```C#
new MoveTo(new Vector3(0, 0, 10), 1.0f).Run(gameObj);
```
Sequence Actions
```C#
new Seq(new List<ActionBase>(){
    new MoveTo(new Vector3(0, 0, 10), 1.0f),
    new RotateTo(new Vector3(0, 90, 0), 0.5f)}).Run(gameObj);
```
Merge Actions
```C#
new Merge(new List<ActionBase>(){
    new MoveTo(new Vector3(0, 0, 10), 1.0f),
    new RotateTo(new Vector3(0, 90, 0), 0.5f)}).Run(gameObj);
```
Make Animation Smoothy By Using Ease Functions
```C#
new MoveTo(new Vector3(0, 0, 10), 1.0f).EaseInSine().Run(gameObj);
```

Actions preview
![My Project Demo](https://raw.githubusercontent.com/fsa2020/UnityActions/main/preview.gif)
