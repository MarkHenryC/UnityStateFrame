using UnityEngine;

/// <summary>
/// A client of this interface, such
/// as a selectable object, will implement
/// this interface to be given controller
/// data when it's relevant to the 
/// object. ControllerQuery is for
/// the object to get info about
/// specifics of touchpad
/// </summary>
interface ISelectable3D
{
    void OnPointerEnter();
    void OnPointerExit();
    void OnTriggerClickDown();
    void OnTriggerClickUp();
    void OnTouchpadClickDown(Vector2 pos);
    void OnTouchpadClickUp(Vector2 pos);
    void OnTouchpadTouchDown(Vector2 pos);
    void OnTouchpadTouchUp(Vector2 pos);
    void Clear();
}
