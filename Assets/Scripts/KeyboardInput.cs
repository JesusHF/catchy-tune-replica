using UnityEngine.InputSystem;

/// <summary>
/// Thin wrapper around the Input System keyboard so gameplay code does not have to
/// null-check <see cref="Keyboard.current"/> on every read (there is no keyboard on
/// some devices, and <see cref="Keyboard.current"/> is null there).
/// </summary>
public static class KeyboardInput
{
    /// <summary>True on the frame the key was pressed, mirroring Input.GetKeyDown.</summary>
    public static bool Pressed(Key key)
    {
        var keyboard = Keyboard.current;
        return keyboard != null && keyboard[key].wasPressedThisFrame;
    }
}
