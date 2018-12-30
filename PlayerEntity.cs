using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerEntity : EntityState
{
    CellBehavior m_PlayerCell;

    /// <summary>
    /// Vector Used To Store The Change In Player Rotation.
    /// </summary>
    Vector3 m_DeltaRotation = Vector3.zero;

    [SerializeField]
    bool m_UseAxisData;

    [SerializeField]
    float m_MouseMoveThreshold = 0.01f;

    [SerializeField]
    float m_RotationSpeedX = 10, 
          m_RotationSpeedY = 10;

    [SerializeField]
    float m_UpDownRotationLimit = 75f;

    internal override void Start()
    {
        base.Start();

        InitializeInputListeners();

        if((m_PlayerCell = GetComponent<CellBehavior>()) == null)
            Debug.LogError("Player Entity Attached To Object Without Cell Behavior.");

        m_PlayerCell.SetCellEntity(this);
    }

    internal bool ConsumeCellEnergy(float energyReq)
    {
        if (energyReq < Energy)
        {
            Energy -= energyReq;
            return true;
        }

        else
            return false;
    }

    void InitializeInputListeners()
    {
        InputEventSink.OnAxisInput += InputEventSink_OnAxisInput;

        InputEventSink.OnKeyHold += InputEventSink_OnKeyHold;
        InputEventSink.OnKeyDown += InputEventSink_OnKeyDown;
        InputEventSink.OnKeyUp += InputEventSink_OnKeyUp;

        InputEventSink.OnMouseClick += InputEventSink_OnMouseClick;
        InputEventSink.OnMouseHold += InputEventSink_OnMouseHold;
        InputEventSink.OnMouseUp += InputEventSink_OnMouseUp;
        InputEventSink.OnMouseMove += InputEventSink_OnMouseMove;
    }

    private void InputEventSink_OnAxisInput(Vector2 input)
    {
        ProcessPlayerAxisInput(input);
    }

    private void InputEventSink_OnMouseMove(MouseInputEventArgs args)
    {
        ModifyPlayerRotation(args.MouseAxisChange);
    }

    ///Should Be In Player Input Handler
    private void ModifyPlayerRotation(Vector2 delta)
    {
        if (Mathf.Abs(delta.y) >= m_MouseMoveThreshold)
            m_DeltaRotation.x -= gameObject.transform.rotation.x
                + ((delta.y * m_RotationSpeedY));

        if (Mathf.Abs(delta.x) >= m_MouseMoveThreshold)
            m_DeltaRotation.y += gameObject.transform.rotation.y
                + ((delta.x * m_RotationSpeedX));

            m_DeltaRotation.x = Mathf.Clamp
                (m_DeltaRotation.x, -m_UpDownRotationLimit, m_UpDownRotationLimit);

            m_DeltaRotation.z = 0;

            gameObject.transform.localRotation = Quaternion.Lerp
            (
                gameObject.transform.rotation,
                Quaternion.Euler(m_DeltaRotation),
                Time.deltaTime / Time.smoothDeltaTime
            );
    }

    private void ProcessPlayerAxisInput(Vector2 input)
    {
        if(m_UseAxisData)
           m_PlayerCell.ProcessAxisInput(input);
    }

    private void InputEventSink_OnMouseUp(MouseInputEventArgs args)
    {
        if (MainEngine.DebugMode)
            Debug.LogFormat("Mouse Button Released: {0}", args.MouseIndex);
    }

    private void InputEventSink_OnMouseHold(MouseInputEventArgs args)
    {
        if (MainEngine.DebugMode)
            Debug.LogFormat("Mouse Button Held: {0}", args.MouseIndex);
    }

    private void InputEventSink_OnMouseClick(MouseInputEventArgs args)
    {
        if (MainEngine.DebugMode)
            Debug.LogFormat("Mouse Button Clicked: {0}", args.MouseIndex);
    }

    private void InputEventSink_OnKeyUp(KeyCode keycode)
    {
        if (MainEngine.DebugMode)
            Debug.LogFormat("Key Released: {0}", keycode.ToString());

        HandleKeyUps(keycode);
    }

    private void InputEventSink_OnKeyDown(KeyCode keycode)
    {
        if (MainEngine.DebugMode)
            Debug.LogFormat("Key Pressed: {0}", keycode.ToString());

        HandleKeyPress(keycode);
    }

    private void InputEventSink_OnKeyHold(KeyCode keycode)
    {
        if (MainEngine.DebugMode)
            Debug.LogFormat("Key Held: {0}", keycode.ToString());

        if(!m_UseAxisData)
            HandleKeyHold(keycode);
    }

    private void HandleKeyPress(KeyCode keyCode)
    {
        switch(keyCode)
        {
            case KeyCode.Space:
                {
                    m_PlayerCell.Jump();
                    break;
                }
            case KeyCode.KeypadEnter:
                {

                    break;
                }
            case KeyCode.LeftShift:
            case KeyCode.RightShift:
                {
                    m_PlayerCell.SetSprintEnabled(true);
                    break;
                }
        }
    }

    private void HandleKeyUps(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.Space:
                {
                    break;
                }
            case KeyCode.KeypadEnter:
                {
                    break;
                }
            case KeyCode.LeftShift:
            case KeyCode.RightShift:
                {
                    m_PlayerCell.SetSprintEnabled(false);
                    break;
                }
        }
    }

    private void HandleKeyHold(KeyCode keyCode)
    {
        switch(keyCode)
        {
            case KeyCode.W:
                {
                    HandleKeyHold_W();
                    break;
                }

            case KeyCode.S:
                {
                    HandleKeyHold_S();
                    break;
                }
            case KeyCode.D:
                {
                    HandleKeyHold_D();
                    break;
                }
            case KeyCode.A:
                {
                    HandleKeyHold_A();
                    break;
                }
        }
    }

    private void HandleKeyHold_W()
    {
       m_PlayerCell.ApplyForwardMomentum();
    }

    private void HandleKeyHold_S()
    {
        m_PlayerCell.ApplyBackwardMomentum();
    }

    private void HandleKeyHold_D()
    {
        m_PlayerCell.ApplyRightMomentum();
    }

    private void HandleKeyHold_A()
    {
        m_PlayerCell.ApplyLeftMomentum();
    }
}
