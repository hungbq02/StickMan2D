using UnityEngine;

// Vận tốc ngang của player trong lúc ra đòn (không phụ thuộc MonoBehaviour => test được).
// Mỗi bước đòn: lao tới (distance / duration) theo hướng mặt, không chậm hơn đà chạy đang có;
// hết thời gian lao thì hãm dần về 0 thay vì đứng khựng.
public struct AttackMotion
{
    private float lungeSpeed;
    private float lungeTimeLeft;

    public bool IsLunging => lungeTimeLeft > 0f;

    public void Begin(float distance, float duration)
    {
        if (distance > 0f && duration > 0f)
        {
            lungeSpeed = distance / duration;
            lungeTimeLeft = duration;
        }
        else
        {
            lungeSpeed = 0f;
            lungeTimeLeft = 0f;
        }
    }

    public void Stop()
    {
        lungeSpeed = 0f;
        lungeTimeLeft = 0f;
    }

    // Vận tốc x cho bước vật lý kế tiếp
    public float Step(float currentVelocityX, int facing, float deltaTime, float deceleration)
    {
        if (lungeTimeLeft > 0f)
        {
            lungeTimeLeft -= deltaTime;
            if (lungeTimeLeft < 1e-5f) lungeTimeLeft = 0f; // sai số float: 0.08 - 4×0.02 không đúng bằng 0
            float carried = currentVelocityX * facing; // đà đang đi cùng hướng mặt (âm nếu ngược)
            return facing * Mathf.Max(lungeSpeed, carried);
        }
        return Mathf.MoveTowards(currentVelocityX, 0f, deceleration * deltaTime);
    }
}
