using System.ComponentModel.DataAnnotations;

namespace AttTest
{
    public class TimerConstants
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int RoundLengthMinSeconds { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int RoundLengthMaxSeconds { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int FocusPointVisibilityLength { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TestLengthSeconds { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int IntroLengthSeconds { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int FalseStartPointVisibleLength { get; set; }
    }
}