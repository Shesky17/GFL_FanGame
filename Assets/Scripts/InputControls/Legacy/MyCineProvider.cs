﻿using Cinemachine;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


/*
 * GetAxisValue() is modified to convert Touch type input to Vector2
 */

/// <summary>
/// This is an add-on to override the legacy input system and read input using the
/// UnityEngine.Input package API.  Add this behaviour to any CinemachineVirtualCamera 
/// or FreeLook that requires user input, and drag in the the desired actions.
/// </summary>
//[HelpURL(Documentation.BaseURL + "manual/CinemachineAlternativeInput.html")]

public class MyCineProvider : MonoBehaviour, AxisState.IInputAxisProvider
{
    /// <summary>
    /// Leave this at -1 for single-player games.
    /// For multi-player games, set this to be the player index, and the actions will
    /// be read from that player's controls
    /// </summary>
    [Tooltip("Leave this at -1 for single-player games.  "
        + "For multi-player games, set this to be the player index, and the actions will "
        + "be read from that player's controls")]
    public int PlayerIndex = -1;

    /// <summary>Vector2 action for XY movement</summary>
    [Tooltip("Vector2 action for XY movement")]
    public InputActionReference XYAxis;

    /// <summary>Float action for Z movement</summary>
    [Tooltip("Float action for Z movement")]
    public InputActionReference ZAxis;



    // Detect if dragging look btn
    private bool isDraggingLookBtn = false;




    /// <summary>
    /// Implementation of AxisState.IInputAxisProvider.GetAxisValue().
    /// Axis index ranges from 0...2 for X, Y, and Z.
    /// Reads the action associated with the axis.
    /// </summary>
    /// <param name="axis"></param>
    /// <returns>The current axis value</returns>
    public virtual float GetAxisValue(int axis)
    {
        var action = ResolveForPlayer(axis, axis == 2 ? ZAxis : XYAxis);
        if (action != null && InputManager.Instance.IsDraggingLookBtn) // && isDraggingLookBtn) //
        {
            //Debug.Log("Dragging Look btn");
            switch (axis)
            {
                case 0:
                    //Debug.Log("UP");
                    return action.ReadValue<Vector2>().x;
                case 1:
                    //Debug.Log("down");
                    // set isDragging to false
                    // how about set it to false when releasing finger from screen (when dragging is finished)
                    //InputManager.Instance.SetDraggingLookBtnVal(false); 
                    return action.ReadValue<Vector2>().y;
                case 2:
                    return action.ReadValue<float>();
            }
        }
        return 0;
    }

    const int NUM_AXES = 3;
    InputAction[] m_cachedActions;

    /// <summary>
    /// In a multi-player context, actions are associated with specific players
    /// This resolves the appropriate action reference for the specified player.
    /// 
    /// Because the resolution involves a search, we also cache the returned 
    /// action to make future resolutions faster.
    /// </summary>
    /// <param name="axis">Which input axis (0, 1, or 2)</param>
    /// <param name="actionRef">Which action reference to resolve</param>
    /// <returns>The cached action for the player specified in PlayerIndex</returns>
    protected InputAction ResolveForPlayer(int axis, InputActionReference actionRef)
    {
        if (axis < 0 || axis >= NUM_AXES)
            return null;
        if (actionRef == null || actionRef.action == null)
            return null;
        if (m_cachedActions == null || m_cachedActions.Length != NUM_AXES)
            m_cachedActions = new InputAction[NUM_AXES];
        if (m_cachedActions[axis] != null && actionRef.action.id != m_cachedActions[axis].id)
            m_cachedActions[axis] = null;
        if (m_cachedActions[axis] == null)
        {
            m_cachedActions[axis] = actionRef.action;
            if (PlayerIndex != -1)
            {
                var user = InputUser.all[PlayerIndex];
                m_cachedActions[axis] = user.actions.First(x => x.id == actionRef.action.id);
            }
        }
        // Auto-enable it if disabled
        if (m_cachedActions[axis] != null && !m_cachedActions[axis].enabled)
            m_cachedActions[axis].Enable();

        return m_cachedActions[axis];
    }

    // Clean up
    protected virtual void OnDisable()
    {
        m_cachedActions = null;
    }

    public void SetDraggingLookBtnVal(bool inVal)
    {
        isDraggingLookBtn = inVal;
    }
}