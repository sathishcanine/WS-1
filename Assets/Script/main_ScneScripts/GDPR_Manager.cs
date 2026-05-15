using UnityEngine;
using GoogleMobileAds.Ump;
using GoogleMobileAds.Ump.Api;
public class GDPR_Manager : MonoBehaviour
{

    ConsentForm _consentForm;

    // Start is called before the first frame update
    void Start()
    {
        // Do not ship ConsentDebugSettings (e.g. forced EEA) to all users — it breaks real
        // geography and an empty TestDeviceHashedIds entry can misconfigure the SDK.
        // Re-enable debug geography only while testing, with real hashed device IDs from Logcat.
        var request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false,
        };

        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    void OnConsentInfoUpdated(FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        if (ConsentInformation.IsConsentFormAvailable())
        {
            LoadConsentForm();
        }
        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.
    }

    void LoadConsentForm()
    {
        // Loads a consent form.
        ConsentForm.Load(OnLoadConsentForm);
    }

    void OnLoadConsentForm(ConsentForm consentForm, FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        // The consent form was loaded.
        // Save the consent form for future requests.
        _consentForm = consentForm;

        // You are now ready to show the form.
        if (ConsentInformation.ConsentStatus == ConsentStatus.Required)
        {
            _consentForm.Show(OnShowForm);
        }
    }


    void OnShowForm(FormError error)
    {
        if (error != null)
        {
            // Handle the error.
            UnityEngine.Debug.LogError(error);
            return;
        }

        // Handle dismissal by reloading form.
        LoadConsentForm();
    }





}
