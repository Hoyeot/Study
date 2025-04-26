using UnityEngine;

public class JumpPropts : MonoBehaviour
{
    private Vector3 _originalScale;
    private float _compressAmount = 10f;
    private float _resetSpeed = 2f;
    private bool _isCompressed = false;

    private void Start()
    {
        _originalScale = transform.localScale;
    }
    private void Update()
    {
        // 압축된 상태에서 서서히 원래 크기로 복구
        if (_isCompressed)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                _originalScale,
                _resetSpeed * Time.deltaTime
            );

            // 거의 원래 크기로 돌아왔을 때
            if (Vector3.Distance(transform.localScale, _originalScale) < 0.01f)
            {
                transform.localScale = _originalScale;
                _isCompressed = false;
            }
        }
    }
    void CompressSpring()
    {
        _isCompressed = true;
        Vector3 compressedScale = _originalScale;
        compressedScale.y *= _compressAmount;
        transform.localScale = compressedScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Player"))
        {
            CompressSpring();
        }
    }
}
