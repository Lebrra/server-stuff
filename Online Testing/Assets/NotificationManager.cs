using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Notification
{
    public string Message;
    public float Duration;
    public bool Interrupt;
    public Color Color;

    public Notification(string message, float duration, bool interrupt, Color color)
    {
        this.Message = message;
        this.Duration = duration;
        this.Interrupt = interrupt;
        this.Color = color;
    }
}

public class NotificationManager : MonoBehaviour
{
    public Text notificationsText;
    public float duration;
    public Color color;

    public bool isNotifying = false;

    public bool interruptMessages;

    public static NotificationManager instance;

    public List<Notification> Notifications;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var newNotification = new Notification("Hello", 3, false, Color.blue);
            addNotification(newNotification);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            var newNotification = new Notification("Interrupting Message", 2, true, Color.red);
            addNotification(newNotification);
        }
        
    }

    public bool addNotification(Notification newNotification)
    {
        if (newNotification.Interrupt)
        {
            StopCoroutine(notify());
            Notifications.Insert(0, newNotification);
            StartCoroutine(notify());
            return true;
        }
        
        Notifications.Add(newNotification);
        if (!isNotifying)
        {
            StartCoroutine(notify());
        }
        return Notifications.Count == 1;
    }

    private IEnumerator notify()
    {
        isNotifying = true;
        while (Notifications.Count > 0)
        {
            var currentNotification = Notifications.First();

            notificationsText.text = currentNotification.Message;
            notificationsText.color = currentNotification.Color;
            yield return new WaitForSeconds(currentNotification.Duration);
            // wait for completion of message to remove it
            Notifications.RemoveAt(0);

            // pause time?
            notificationsText.text = "";
            yield return new WaitForSeconds(1);
            yield return false;
        }

        print("Notifications finised");
        isNotifying = false;
        yield return true;
    }
}
