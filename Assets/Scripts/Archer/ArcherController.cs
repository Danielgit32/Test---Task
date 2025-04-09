using System.Collections.Generic;
using UnityEngine;

public class ArcherController : MonoBehaviour
{
    public GameObject _arrowPrefab;
    public Transform _arrowSpawnPoint;
    public float _arrowSpeed = 20f;
    public float _maxChargeTime = 2f;  // Максимальное время зарядки
    public float _minArrowSpeed = 10f; // Минимальная скорость стрелы
    public float _maxArrowSpeed = 30f; // Максимальная скорость стрелы
    public float _rotationSpeed = 100f; // Скорость вращения лучника

    public GameObject _trajectoryPointPrefab; // Префаб точки траектории
    public float _timeBetweenTrajectoryPoints = 0.1f; // Интервал между точками
    public Transform _trajectoryHolder;

    private List<GameObject> _trajectoryPoints = new List<GameObject>();
    private bool _isCharging = false;
    private float _chargeStartTime;
    private float _currentChargeTime = 0f;
    private int _numberOfTrajectoryPoints = 5; // Количество точек траектории
    private float _minAngle = -60f; 
    private float _maxAngle = 60f; 

    private void Start()
    {
        // Создаем точки траектории
        for (int i = 0; i < _numberOfTrajectoryPoints; i++)
        {
            GameObject point = Instantiate(_trajectoryPointPrefab, _trajectoryHolder);
            point.SetActive(false);
            _trajectoryPoints.Add(point);
        }
    }

    private void Update()
    {
        // Вращение лучника в направлении мыши
        RotateTowardsMouse();

        // Зарядка лука
        if (Input.GetMouseButtonDown(0)) // Левая кнопка мыши нажата
        {
            StartCharging();
        }

        if (Input.GetMouseButton(0) && _isCharging) // Левая кнопка мыши удерживается
        {
            UpdateCharge();
            ShowTrajectory();
        }
        else
        {
            HideTrajectory();
        }

        // Выстрел
        if (Input.GetMouseButtonUp(0) && _isCharging) // Левая кнопка мыши отпущена
        {
            Shoot();
        }
    }

    private void RotateTowardsMouse()
    {
        // Получаем координаты мыши в мировом пространстве
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

        // Вычисляем направление от лучника к мыши
        Vector3 direction = worldPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Ограничиваем угол поворота
        angle = Mathf.Clamp(angle, _minAngle, _maxAngle);

        // Применяем поворот
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    private void StartCharging()
    {
        _isCharging = true;
        _chargeStartTime = Time.time;
        _currentChargeTime = 0f;
    }

    private void UpdateCharge()
    {
        _currentChargeTime = Mathf.Clamp(Time.time - _chargeStartTime, 0f, _maxChargeTime);
    }

    private void Shoot()
    {
        _isCharging = false;
        HideTrajectory();

        // Вычисляем скорость стрелы на основе времени зарядки
        float chargeRatio = Mathf.Clamp01(_currentChargeTime / _maxChargeTime);
        float finalArrowSpeed = Mathf.Lerp(_minArrowSpeed, _maxArrowSpeed, chargeRatio);

        // Создаем стрелу
        GameObject arrow = Instantiate(_arrowPrefab, _arrowSpawnPoint.position, _arrowSpawnPoint.rotation);
        Rigidbody2D arrowRb = arrow.GetComponent<Rigidbody2D>();

        // Задаем скорость стреле
        if (arrowRb != null)
        {
            arrowRb.linearVelocity = _arrowSpawnPoint.right * finalArrowSpeed;
        }
        else
        {
            Debug.LogError("Arrow prefab does not have a Rigidbody2D component!");
        }

        Destroy(arrow, 5f);
    }

    private void ShowTrajectory()
    {
        float chargeRatio = Mathf.Clamp01(_currentChargeTime / _maxChargeTime);
        float finalArrowSpeed = Mathf.Lerp(_minArrowSpeed, _maxArrowSpeed, chargeRatio);
        Vector2 startPosition = _arrowSpawnPoint.position;
        Vector2 velocity = _arrowSpawnPoint.right * finalArrowSpeed;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        for (int i = 0; i < _numberOfTrajectoryPoints; i++)
        {
            float time = _timeBetweenTrajectoryPoints * i;
            Vector2 position = CalculateTrajectoryPoint(startPosition, velocity, time, gravity);

            if (i < _trajectoryPoints.Count)
            {
                _trajectoryPoints[i].SetActive(true);
                _trajectoryPoints[i].transform.position = position;
            }
        }
    }

    private void HideTrajectory()
    {
        foreach (GameObject point in _trajectoryPoints)
        {
            point.SetActive(false);
        }
    }

    private Vector2 CalculateTrajectoryPoint(Vector2 startPosition, Vector2 velocity, float time, float gravity)
    {
        return startPosition + velocity * time + 0.5f * new Vector2(0, -gravity) * time * time;
    }
}
