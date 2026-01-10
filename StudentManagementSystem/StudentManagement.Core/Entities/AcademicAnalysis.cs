using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


        namespace StudentManagement.Core.Entities
        {
            public class AcademicAnalysis
            {
                [Key]
                public int AnalysisId { get; set; }

                public int StudentId { get; set; }
                [ForeignKey("StudentId")]
                public Student Student { get; set; }

                public DateTime AnalysisDate { get; set; } = DateTime.Now;
                public double OverallGPA { get; set; }

                // Lưu JSON string
                public string StrongSubjectsJson { get; set; } // ["Math", "Physics"]
                public string WeakSubjectsJson { get; set; }   // ["History"]

                public string Recommendations { get; set; } // "Nên tập trung..."
                public string AiModelUsed { get; set; } // "GPT-4" or "Fallback"
            }
        }

