Merging
=======

Unfortunately, when initially setting up our Unity project, I forgot that it is not by default set up to be managed by external source control.
That means our current scene and asset files are binary, which means they're impossible to merge!

Everything in master is currently set up correctly, but to merge the changes into your branch, you'll need to take similar steps and commit it first.

1. Ensure your branch with your work is checked out.
2. Open *Edit > Project Settings > Editor*.
3. Set *Version Control Mode* to *Visible Meta Files*, and *Asset Serialization Mode* to *Force Text*.
4. This will not by default re-serialize your scene file. So, go make a change (like, change an object's Transform by 1 unit or something), save the scene, undo your change, then save it again. Go open up the `Scene.unity` file in a text editor and ensure it's YAML, not binary gibberish.
5. Open up `UnityProject/.gitignore` and remove the entries for `Assets` (and anything in Assets) and `ProjectSettings`.
6. Run `$ git config --edit` in the project, and add:
```
[merge]
    tool = unityyamlmerge
```
Next:

1. Follow the instructions for Git here: http://docs.unity3d.com/Manual/SmartMerge.html but put the `[mergetool]` stuff in the file that opens when you run `git config --global --edit`.
2. Follow the instructions in `<wherever you have Unity installed>\Editor\Data\Tools\mergespecfile.txt` to set up a fallback merge tool (like KDiff3 or whatever) for scene and prefab files
3. Commit everything you added in your branch, then (after ensuring your local `master` branch is up to date via `git pull`) run `$ git merge master`.
4. Run `git mergetool`, and follow the prompts. When it gets to the scene file, it'll likely tell you that the BASE file is binary, which means you'll need to do the merge manually.

Merging the scene file manually:

1. Reset the merge you're probably in right now.
2. Check out `master`, and copy the `Scene.unity` file somewhere safe. Rename it to `Scene.master.unity` or whatever.
3. Check out your branch, and copy the `Scene.unity` file to that same place. Rename it to `Scene.dest.unity` or whatever.
4. Use a diff/merge program (again, like KDiff3) to do the merge. Hopefully not much is different? Name the resulting file `Scene.unity`, of course.
5. Do the merge again, accept that UnityYAMLMerge can't do anything with `Scene.unity`, then copy your newly merged file back into the folder.
6. Add everything, then finish the merge (`$ git commit -a`)
