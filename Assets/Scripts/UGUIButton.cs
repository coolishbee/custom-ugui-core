using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine;

namespace CustomUI
{
    public class UGUIButton : Selectable, IPointerClickHandler, ISubmitHandler
    {
        public enum Sound
        {
            none,

            click_1,
            click_2,
        }
        public Sound sound = Sound.click_1;
        public bool pressScaling = true;
        Vector3 orgScale = new Vector3(1f, 1f, 1f);

        [Serializable]
        public class ButtonClickedEvent : UnityEvent { }

        // Event delegates triggered on click.
        [FormerlySerializedAs("onClick")]
        [SerializeField]
        private ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

        protected UGUIButton()
        {
        }

        public ButtonClickedEvent onClick
        {
            get { return m_OnClick; }
            set { m_OnClick = value; }
        }

        static public void PlaySound(Sound snd, GameObject go)
        {
            if (snd != Sound.none)
            {
                switch (snd)
                {
                    case Sound.click_1: CustomUtil.Play("click1", go); break;
                }
            }
        }

        private void Press()
        {
            if (!IsActive() || !IsInteractable())
                return;

            UISystemProfilerApi.AddMarker("Button.onClick", this);
            m_OnClick.Invoke();
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            PlaySound(sound, gameObject);
            Press();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (pressScaling)
            {
                
                Debug.Log("OnPointerDown : " + IsPressed());
                if (IsPressed())
                {
                    float duration = 0.05f;
                    Vector3 vScale = gameObject.transform.localScale * 1.05f;

                    iTween.ScaleTo(gameObject, iTween.Hash(
                        "scale", vScale,
                        "time", duration,
                        "easetype", "easeInOutBack",
                        "looptype", "none"
                    ));
                }
            }
        }
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (pressScaling)
            {
                
                Debug.Log("OnPointerUp : " + IsPressed());
                if (IsPressed() == false)
                {
                    float duration = 0.05f;

                    iTween.ScaleTo(gameObject, iTween.Hash(
                        "scale", orgScale,
                        "time", duration,
                        "easetype", "easeInOutBack",
                        "looptype", "none"
                    ));
                }
            }
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Press();

            // if we get set disabled during the press
            // don't run the coroutine.
            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());
        }

        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }
    }
}
