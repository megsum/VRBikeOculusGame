using UnityEngine;
using System.Collections;

/// <summary>
/// Defines our level IDs all in one place to keep things DRY.
/// 
/// IMPORTANT: if the order of the scenes change in the project's Build Settings,
/// this will need to be updated, or Extremely Strange Things will likely occur.
/// </summary>
public enum Levels
{ 
    StartScreen = 0,
    Trail = 1,
    HalfwayScreen = 2,
    EndScreen = 3
}
