using System;
using UniInject;
using UniRx;

class ChaseTheCrownPlayerControl : INeedInjection, IInjectionFinishedListener
{

    [Inject]
    private PlayerPerformanceAssessmentControl playerPerformanceAssessmentControl;

    [Inject]
    private SingSceneControl singSceneControl;

    public ChaseTheCrownModSettings modSettings;

    public Action onCrownCollected;

    public bool IsCrownOwner { get; set; }
    public int perfectNotes = 0;

    public void OnInjectionFinished()
    {

        playerPerformanceAssessmentControl.NoteAssessedEventStream.Subscribe(noteScoreEvent =>
        {
            if (!IsCrownOwner)
            {
                double bpm = singSceneControl.SongMeta.BeatsPerMinute;
                double msPerBeat = 60 / bpm * 1000;
                double msPerNote = noteScoreEvent.Note.Length * msPerBeat;

                if (msPerNote > 140)
                {
                    // only count notes that are long enough 
                    if (noteScoreEvent.IsPerfect)
                    {
                        perfectNotes++;
                    }
                }

                if (!noteScoreEvent.IsPerfect)
                {
                    // reset perfect note count if a note is missed
                    perfectNotes = 0;
                }

                if (perfectNotes >= modSettings.getCrownOnThisManyPerfectNotes)
                {
                    IsCrownOwner = true;
                    onCrownCollected.Invoke();
                    perfectNotes = 0;
                }

            }

        });

    }
}