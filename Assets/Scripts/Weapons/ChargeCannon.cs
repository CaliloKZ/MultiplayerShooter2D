using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MEC;
using DG.Tweening;

public class ChargeCannon : Weapon
{
    private GameObject m_bullet;
    private bool m_isCharging;

    [SerializeField] private float m_chargeTime;

    protected override void OnDisable()
    {
        if (m_isCharging)
        {
            m_isCharging = false;
            StopCharge();
        }
        base.OnDisable();
    }
    protected override void PoolOnGet(GameObject obj)
    {
        obj.GetComponent<CircleCollider2D>().enabled = false;
        obj.GetComponent<SpriteRenderer>().color = Color.green;
        obj.transform.localScale = _bulletPrefab.transform.localScale;
        obj.SetActive(true);
    }

    protected override void Update()
    {
        if (_isShooting && Time.time >= _nextTimeToFire)
        {
            Charge();
        }
    }

    protected override void ShootInput(InputAction.CallbackContext context)
    {
        base.ShootInput(context);

        if (context.canceled && m_isCharging)
             StopCharge();
    }

    protected void Charge()
    {
        if (_currentAmmo <= 0)
            return;

        if (!m_isCharging) {
            m_bullet = _bulletPool.Get();
            Timing.RunCoroutine(Charging().CancelWith(gameObject), "chargeRoutine");
        }
        m_isCharging = true;
        m_bullet.transform.position = _firePoint.position;
        m_bullet.transform.rotation = _firePoint.rotation;
    }

    protected override void Shoot()
    {
        _nextTimeToFire = Time.time + _fireRate;

        _currentAmmo--;
        AmmoCount.OnCurrentAmmoChange(_currentAmmo);

        m_bullet.GetComponent<Rigidbody2D>().AddForce(_firePoint.right * _bulletSpeed, ForceMode2D.Impulse);
        m_bullet.GetComponent<CircleCollider2D>().enabled = true;

        Bullet bulletScript = m_bullet.GetComponent<Bullet>();
        bulletScript.SetDamage(_damage);
        bulletScript.Init(KillBullet);

        m_isCharging = false;
    }

    IEnumerator<float> Charging()
    {
        Debug.Log("test call");
        m_bullet.transform.DOScale(1, m_chargeTime);
        m_bullet.GetComponent<SpriteRenderer>().DOColor(Color.red, m_chargeTime);
        yield return Timing.WaitForSeconds(m_chargeTime);
        Shoot();
    }

    private void StopCharge()
    {
        m_isCharging = false;
        DOTween.Kill(m_bullet, false);
        DOTween.Clear();
        Timing.KillCoroutines("chargeRoutine");
        KillBullet(m_bullet.GetComponent<Bullet>());
    }


}