namespace SiteAgendamento.Models
{
    public class AgendamentoVM
    {
        public int Id { get; set; }

        public DateTime DtHoraAgendamento { get; set; }

        public DateOnly DataAtendimento { get; set; }

        public TimeOnly Horario { get; set; }

        public int FkUsuarioId { get; set; }

        public int FkServicoId { get; set; }
    }
}
