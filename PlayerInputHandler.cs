using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable
public class MouseInputEventArgs
{
    internal int m_MouseIndex;
    internal Vector2 m_MouseAxisChange;
    internal Vector3 m_MousePosition;

    public int MouseIndex { get { return m_MouseIndex; } }
    public Vector2 MouseAxisChange { get { return m_MouseAxisChange; } }
    public Vector3 MouseScreenPosition { get { return m_MousePosition; } }

    public MouseInputEventArgs(int index, Vector2 mouseChg, Vector3 mousePos)
    {
        m_MouseIndex = index;
        m_MouseAxisChange = mouseChg;
        m_MousePosition = mousePos;
    }
}

public class AxisInputEventArgs
{
    internal Vector2 m_Input;

    public Vector2 Input { get { return m_Input; } }

    public AxisInputEventArgs(Vector2 input)
    {
        m_Input = input;
    }
}

public class PlayerInputHandler : MonoBehaviour
{
    [SerializeField]
    private bool m_IndependentDebugging;
    [SerializeField]
    private bool m_UseGamePad = false;

    [SerializeField]
    private float m_MouseJoyDeltaThreshold = 0.001f;

    [SerializeField]
    private Slider m_HorizontalSlider;
    [SerializeField]
    private Slider m_VerticalSlider;

    Vector2 m_LastMouseDelta;
    Vector2 m_LastJoyDelta;

    public Vector2 LastMouseDelta { get { return m_LastMouseDelta; } }

    public static readonly int LEFTMOUSEBUTTONINDEX = 0;
    public static readonly int RIGHTMOUSEBUTTONINDEX = 1;

    public float HorizontalSensitivity
    {
        get { return m_HorizontalSlider.value; }
    }

    public float VerticalSensitivity
    {
        get { return m_VerticalSlider.value; }
    }

    internal static KeyCode[] m_TrackedKeys =
    {
        KeyCode.W,  //Move Forward
        KeyCode.A,  //Strafe Left 
        KeyCode.S,  //Strafe Right
        KeyCode.D,  //Move Backwards

        KeyCode.C,  //Construct

        KeyCode.Space,          //Jump
        KeyCode.LeftShift,      //Sprint    
        KeyCode.RightShift,
        KeyCode.Tab,            //Cycle Weapons

        KeyCode.Return,         //Display Text Field
        KeyCode.Escape,         //Pause

        KeyCode.JoystickButton0, // [PS3 : X        ]
        KeyCode.JoystickButton1, // [PS3 : Square   ]
        KeyCode.JoystickButton2, // [PS3 : Circle   ]
        KeyCode.JoystickButton3, // [PS3 : Triangle ]
        KeyCode.JoystickButton4, // [PS3 : L1       ]
        KeyCode.JoystickButton5, // [PS3 : R1       ]
        KeyCode.JoystickButton7, // [PS3 : Start    ]
        KeyCode.JoystickButton8, // [PS3 : L3       ]
        KeyCode.JoystickButton9, // [PS3 : R3       ]
    };

    internal static int[] m_MouseButtonIndices = { LEFTMOUSEBUTTONINDEX, RIGHTMOUSEBUTTONINDEX };


    static KeyCode[] KeyCodes = ((KeyCode[])Enum.GetValues(typeof(KeyCode)));
    private void Start()
    {
        if (m_IndependentDebugging)
        {
            foreach (KeyCode c in KeyCodes)
            {
                Debug.LogFormat("Value Name: {0}", c.ToString());
            }

            for(int i = Input.GetJoystickNames().Length -1; i >= 0; i--)
            {
                Debug.LogFormat("JoyStick [{0}] {1}", i, Input.GetJoystickNames()[i]);
            }
        }
    }

    void Update()
    {
        if (MainEngine.Instance.GamePaused)
            return;

        ProcessAxisMovement();
        ProcessMousePosition();

        if(m_UseGamePad)
            ProcessJoystickPosition();

        if(m_IndependentDebugging)
            for(int i = KeyCodes.Length -1; i >= 0; i--)
            {
                if (Input.GetKey(KeyCodes[i]))
                    Debug.LogFormat
                        ("{0} ({1}) Has Been Pressed!", i, KeyCodes[i].ToString());
            }

        for (int i = m_TrackedKeys.Length - 1; i >= 0; i--)
        {
            if (Input.GetKeyDown(m_TrackedKeys[i]))
                HandleKeyDown(m_TrackedKeys[i]);

            if (Input.GetKey(m_TrackedKeys[i]))
                HandleKeyHold(m_TrackedKeys[i]);

            if (Input.GetKeyUp(m_TrackedKeys[i]))
                HandleKeyUp(m_TrackedKeys[i]);
        }

        for (int i = m_MouseButtonIndices.Length - 1; i >= 0; i--)
        {
            if (Input.GetMouseButtonDown(m_MouseButtonIndices[i]))
                HandleMouseClick
                    (new MouseInputEventArgs(m_MouseButtonIndices[i], m_LastMouseDelta, Input.mousePosition));

            if (Input.GetMouseButton(m_MouseButtonIndices[i]))
                HandleMouseHold
                    (new MouseInputEventArgs(m_MouseButtonIndices[i], m_LastMouseDelta, Input.mousePosition));

            if (Input.GetMouseButtonUp(m_MouseButtonIndices[i]))
                HandleMouseRelease
                    (new MouseInputEventArgs(m_MouseButtonIndices[i], m_LastMouseDelta, Input.mousePosition));
        }
    }

    void ProcessAxisMovement()
    {
        Vector2 input; ///Optimize

        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        InputEventSink.InvokeAxisInputEvent(input);

        if (m_IndependentDebugging)
            Debug.Log("Current Axis Input: " + input.ToString("r7"));
    }

    private void ProcessMousePosition()
    {
        Vector2 input;

        input.x = Input.GetAxisRaw("Mouse X");
        input.y = Input.GetAxisRaw("Mouse Y");

        if (Mathf.Abs(input.x) >= m_MouseJoyDeltaThreshold)
            m_LastMouseDelta.x = input.x;
        else
            m_LastMouseDelta.x = 0;

        if (Mathf.Abs(input.y) >= m_MouseJoyDeltaThreshold)
            m_LastMouseDelta.y = input.y;
        else
            m_LastMouseDelta.y = 0;

        m_LastMouseDelta.x = (m_LastMouseDelta.x * HorizontalSensitivity);
        m_LastMouseDelta.y = (m_LastMouseDelta.y * VerticalSensitivity);

        m_LastJoyDelta = Vector2.zero;

        InputEventSink.InvokeOnMouseMove
            (new MouseInputEventArgs(-1, m_LastMouseDelta, Input.mousePosition));

        if (MainEngine.DebugMode || m_IndependentDebugging)
            Debug.Log("Last Mouse Delta: " + m_LastMouseDelta.ToString());
    }

    private void ProcessJoystickPosition()
    {
        Vector2 input;

        input.x = Input.GetAxis("Joy X");
        input.y = Input.GetAxis("Joy Y");

        if (Mathf.Abs(input.x) >= m_MouseJoyDeltaThreshold)
            m_LastJoyDelta.x = input.x;
        else
            m_LastJoyDelta.x = 0;

        if (Mathf.Abs(input.y) >= m_MouseJoyDeltaThreshold)
            m_LastJoyDelta.y = input.y;
        else
            m_LastJoyDelta.y = 0;

        m_LastJoyDelta.x = m_LastJoyDelta.x * HorizontalSensitivity;
        m_LastJoyDelta.y = m_LastJoyDelta.y * VerticalSensitivity;

        m_LastMouseDelta = Vector2.zero;

        InputEventSink.InvokeOnMouseMove
            (new MouseInputEventArgs(-1, m_LastJoyDelta, Input.mousePosition));

        if (MainEngine.DebugMode || m_IndependentDebugging)
            Debug.Log("Current Joystick Output: " + m_LastJoyDelta.ToString());
    }

    private void HandleMouseRelease(MouseInputEventArgs args)
    {
        InputEventSink.InvokeMouseUpEvent(args);
    }

    private void HandleMouseHold(MouseInputEventArgs args)
    {
        InputEventSink.InvokeMouseHoldEvent(args);
    }

    private void HandleMouseClick(MouseInputEventArgs args)
    {
        InputEventSink.InvokeMouseClickEvent(args);
    }

    private void HandleKeyUp(KeyCode keyCode)
    {
        InputEventSink.InvokeKeyUpEvent(keyCode);
    }

    private void HandleKeyDown(KeyCode keyCode)
    {
        InputEventSink.InvokeKeyDownEvent(keyCode);
    }

    private void HandleKeyHold(KeyCode keyCode)
    {
        InputEventSink.InvokeKeyHoldEvent(keyCode);
    }
}

public class InputEventSink
{
    public delegate void AxisInputEvent(Vector2 input);

    public static event AxisInputEvent OnAxisInput;

    public static void InvokeAxisInputEvent(Vector2 input)
    {
        if (OnAxisInput != null)
            OnAxisInput.Invoke(input);
    }

    public delegate void MousePositionEvent(MouseInputEventArgs args);

    public static event MousePositionEvent OnMouseMove;

    public static void InvokeOnMouseMove(MouseInputEventArgs args)
    {
        if (OnMouseMove != null)
            OnMouseMove.Invoke(args);
    }

    public delegate void KeyDownEvent(KeyCode keycode);

    public static event KeyDownEvent OnKeyDown;

    public static void InvokeKeyDownEvent(KeyCode keyCode)
    {
        if (OnKeyDown != null)
            OnKeyDown.Invoke(keyCode);
    }

    public delegate void KeyHoldEvent(KeyCode keycode);

    public static event KeyHoldEvent OnKeyHold;

    public static void InvokeKeyHoldEvent(KeyCode keyCode)
    {
        if (OnKeyHold != null)
            OnKeyHold.Invoke(keyCode);
    }

    public delegate void KeyUpEvent(KeyCode keycode);

    public static event KeyUpEvent OnKeyUp;

    public static void InvokeKeyUpEvent(KeyCode keyCode)
    {
        if (OnKeyUp != null)
            OnKeyUp.Invoke(keyCode);
    }

    public delegate void MouseClickEvent(MouseInputEventArgs args);

    public static event MouseClickEvent OnMouseClick;

    public static void InvokeMouseClickEvent(MouseInputEventArgs args)
    {
        if (OnMouseClick != null)
            OnMouseClick.Invoke(args);
    }

    public delegate void MouseHoldEvent(MouseInputEventArgs args);

    public static event MouseHoldEvent OnMouseHold;

    public static void InvokeMouseHoldEvent(MouseInputEventArgs args)
    {
        if (OnMouseHold != null)
            OnMouseHold.Invoke(args);
    }

    public delegate void MouseUpEvent(MouseInputEventArgs args);

    public static event MouseUpEvent OnMouseUp;

    public static void InvokeMouseUpEvent(MouseInputEventArgs args)
    {
        if (OnMouseUp != null)
            OnMouseUp.Invoke(args);
    }
}
