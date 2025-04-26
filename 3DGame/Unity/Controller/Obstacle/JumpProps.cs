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
        // ����� ���¿��� ������ ���� ũ��� ����
        if (_isCompressed)
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale,
                _originalScale,
                _resetSpeed * Time.deltaTime
            );

            // ���� ���� ũ��� ���ƿ��� ��
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
