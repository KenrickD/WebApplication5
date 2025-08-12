using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    [Table("TB_OTPSetting")]
    public class OTPSetting
    {
        public Guid OTPId { get; set; }
        public string? OTPAction { get; set; }
        public bool Email { get; set; }
        public bool Whatsapp { get; set; }

    }
}
