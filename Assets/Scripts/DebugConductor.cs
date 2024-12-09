using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugConductor : MonoBehaviour
{
    [SerializeField] private Conductor _conductor;
    [SerializeField] private TextMeshProUGUI _songPosition;

    private void Start()
    {

    }

    private void Update()
    {
        if (_conductor != null)
        {
            _songPosition.text = string.Format("{00:0.00}", _conductor.songPositionInBeats);
        }

        if (true)
        {

        }
    }
}
