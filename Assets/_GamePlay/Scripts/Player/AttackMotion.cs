using UnityEngine;

// Vận tốc ngang của player trong lúc ra đòn (không phụ thuộc MonoBehaviour => test được).
// Mỗi bước đòn: bước tới (distance / duration) theo hướng mặt; tốc độ trong lúc đánh luôn ≤ maxSpeed
// (đang chạy vào đòn => tụt ngay về trần, không trượt theo đà chạy); hết thời gian bước thì hãm dần về 0.
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
    public float Step(float currentVelocityX, int facing, float deltaTime, float deceleration, float maxSpeed)
    {
        float current = Mathf.Clamp(currentVelocityX, -maxSpeed, maxSpeed);
        if (lungeTimeLeft > 0f)
        {
            lungeTimeLeft -= deltaTime;
            if (lungeTimeLeft < 1e-5f) lungeTimeLeft = 0f; // sai số float: 0.08 - 4×0.02 không đúng bằng 0
            return facing * Mathf.Min(lungeSpeed, maxSpeed);
        }
        return Mathf.MoveTowards(current, 0f, deceleration * deltaTime);
    }
}
