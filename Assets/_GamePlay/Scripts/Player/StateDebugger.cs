using UnityEngine;

public class StateDebugger : MonoBehaviour
{
    private PlayerStateMachine stateMachine;
    public Vector2 offset = new Vector2(0, 2f); // Để text nổi lên trên player

    void Start()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
    }

    void OnGUI()
    {
        if (stateMachine == null || stateMachine.CurrentState == null) return;

        Vector3 worldPos = transform.position + (Vector3)offset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // Lật Y để hiển thị đúng vị trí trong GUI
        screenPos.y = Screen.height - screenPos.y;

        GUIStyle style = new GUIStyle();
        style.fontSize = 50;
        style.normal.textColor = Color.green;
        GUI.Label(new Rect(screenPos.x, screenPos.y, 400, 90), stateMachine.CurrentState.GetType().Name, style);
    }
}
