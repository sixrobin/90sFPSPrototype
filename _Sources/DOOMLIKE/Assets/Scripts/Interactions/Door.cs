using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] private Animator _animator = null;

    private bool _isOpen = false;

    public void OpenClose()
    {
        _isOpen = !_isOpen;
        _animator.SetTrigger(_isOpen ? "Open" : "Close");
    }
}