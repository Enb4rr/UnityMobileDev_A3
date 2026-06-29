using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
    #region Variables
    [Header("Target Settings")]
    [SerializeField] private int _scoreValue = 1;
    [SerializeField] private float _lifeTime = 1.5f;

    [Header("Visual Feedback")]
    [SerializeField] private Image _targetImage;

    private Button _button;
    private bool _wasTapped;

    #endregion

    private void Awake()
    {
        _button = GetComponent<Button>();

        if (_targetImage == null) _targetImage = GetComponent<Image>();

        _button.onClick.AddListener(OnTargetTapped);
    }

    private void OnEnable()
    {
        _wasTapped = false;
        StartCoroutine(LifeRoutine());
    }

    private void OnTargetTapped()
    {
        if (_wasTapped) return;

        _wasTapped = true;
        ReactionGameManager.Instance.AddScore(_scoreValue);
        Destroy(gameObject);
    }

    private IEnumerator LifeRoutine()
    {
        float timer = 0f;

        while (timer < _lifeTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!_wasTapped)
        {
            ReactionGameManager.Instance.ReduceLives();
            Destroy(gameObject);
        }
    }
}