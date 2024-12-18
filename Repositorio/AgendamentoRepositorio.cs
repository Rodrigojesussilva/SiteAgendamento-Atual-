using SiteAgendamento.ORM;
using SiteAgendamento.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace SiteAgendamento.Repositorio
{
    public class AgendamentoRepositorio
    {
        private readonly BdElysiumContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Construtor
        public AgendamentoRepositorio(BdElysiumContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;  // Injeção de IHttpContextAccessor
        }

        // Método para inserir um novo agendamento
        public bool InserirAgendamento(DateTime dtHoraAgendamento, DateOnly dataAtendimento, TimeOnly horario, int fkUsuarioId, int fkServicoId)
        {
            try
            {
                // Criando uma instância do modelo AtendimentoVM
                var atendimento = new TbAgendamento
                {
                    DtHoraAgendamento = dtHoraAgendamento,
                    DataAtendimento = dataAtendimento,
                    Horario = horario,
                    FkUsuarioId = fkUsuarioId,
                    FkServicoId = fkServicoId
                };

                // Adicionando o atendimento ao contexto
                _context.TbAgendamentos.Add(atendimento);
                _context.SaveChanges(); // Persistindo as mudanças no banco de dados

                return true; // Retorna true indicando sucesso
            }
            catch (Exception ex)
            {
                // Em caso de erro, pode-se logar a exceção (ex.Message)
                return false; // Retorna false em caso de erro
            }
        }

        // Método para listar todos os atendimentos
        public List<ViewAgendamentoVM> ListarAgendamentos()
            {
                List<ViewAgendamentoVM> listAtendimentos = new List<ViewAgendamentoVM>();

                // Recuperando todos os atendimentos do DbSet
                var listTb = _context.ViewAgendamentos.ToList();

                // Convertendo os atendimentos de TbAtendimento para AtendimentoVM
                foreach (var item in listTb)
                {
                    var atendimento = new ViewAgendamentoVM
                    {
                        Id = item.Id,
                        DtHoraAgendamento = item.DtHoraAgendamento,
                        DataAtendimento = item.DataAtendimento,
                        Horario = item.Horario,
                        TipoServico = item.TipoServico,
                        Valor = item.Valor,
                        Nome = item.Nome,
                        Email = item.Email,
                        Telefone = item.Telefone,

                    };

                    listAtendimentos.Add(atendimento);
                }

                return listAtendimentos;
            }
        public List<ViewAgendamentoVM> ListarAgendamentosCliente()
        {
            // Obtendo o ID do usuário a partir da variável de ambiente
            string nome = _httpContextAccessor.HttpContext.Session.GetString("USUARIO_NOME");            

            List<ViewAgendamentoVM> listAtendimentos = new List<ViewAgendamentoVM>();

            // Recuperando todos os agendamentos que correspondem ao ID do usuário
            var listTb = _context.ViewAgendamentos.Where(x => x.Nome == nome).ToList();

            // Convertendo cada agendamento para ViewAgendamentoVM
            foreach (var item in listTb)
            {
                var atendimento = new ViewAgendamentoVM
                {
                    Id = item.Id,
                    DtHoraAgendamento = item.DtHoraAgendamento,
                    DataAtendimento = item.DataAtendimento,
                    Horario = item.Horario,
                    TipoServico = item.TipoServico,
                    Valor = item.Valor,
                    Nome = item.Nome,
                    Email = item.Email,
                    Telefone = item.Telefone,
                };

                listAtendimentos.Add(atendimento);
            }

            return listAtendimentos;
        }
        // Método para atualizar um atendimento
        public bool AlterarAgendamento(int id, string data, int servico, TimeOnly horario)
            {
                try
                {
                    TbAgendamento agt = _context.TbAgendamentos.Find(id);
                    DateOnly dtHoraAgendamento;
                    if (agt != null)
                    {
                        agt.Id = id;
                        if (data != null)
                        {
                            if (DateOnly.TryParse(data, out dtHoraAgendamento))
                            {
                                agt.DataAtendimento = dtHoraAgendamento;
                            }
                        }

                        // Corrigido a verificação do tipo TimeOnly
                        if (horario != TimeOnly.MinValue)  // Verificando se o horário não é o valor padrão
                        {
                            agt.Horario = horario;
                        }

                        agt.FkServicoId = servico;
                        _context.SaveChanges();
                        return true;
                    }

                    return false;
                }
                catch (Exception)
                {
                    return false;
                }
            }       
        public bool ExcluirAgendamento(int id)
        {
            try
            {
               

               var agt = _context.TbAgendamentos.Where(a => a.Id == id).FirstOrDefault();
                if (agt != null)
                {
                    _context.TbAgendamentos.Remove(agt);

                }
                _context.SaveChanges();
                return true;
            }

            catch (Exception)
            {

                return false;
            }
        }
        public List<AgendamentoVM> ConsultarAgendamento(string datap)
        {
            if (string.IsNullOrEmpty(datap))
            {
                // Se o parâmetro for vazio ou nulo, retornamos uma lista vazia ou podemos tratar conforme necessário
                Console.WriteLine("O parâmetro 'datap' está vazio ou nulo.");
                return new List<AgendamentoVM>(); // Retorna uma lista vazia
            }

            try
            {
                // Tenta converter a string para DateOnly, caso contrário retorna uma lista vazia
                DateOnly data = DateOnly.ParseExact(datap, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                string dataFormatada = data.ToString("yyyy-MM-dd"); // Formato desejado: "yyyy-MM-dd"
                Console.WriteLine(dataFormatada);

                // Consulta ao banco de dados, convertendo para o tipo AgendamentoVM
                var ListaAgendamento = _context.TbAgendamentos
                    .Where(a => a.DataAtendimento == DateOnly.Parse(dataFormatada))
                    .Select(a => new AgendamentoVM
                    {
                        // Mapear as propriedades de TbAgendamento para AgendamentoVM
                        Id = a.Id,
                        DtHoraAgendamento = a.DtHoraAgendamento,
                        DataAtendimento = DateOnly.Parse(dataFormatada),
                        Horario = a.Horario,
                        FkUsuarioId = a.FkUsuarioId,
                        FkServicoId = a.FkServicoId
                    })
                    .ToList(); // Converte para uma lista

                return ListaAgendamento;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao consultar agendamentos: {ex.Message}");
                return new List<AgendamentoVM>(); // Retorna uma lista vazia em caso de erro
            }
        }
        public List<UsuarioVM> ListarNomesAgendamentos()
        {
            // Lista para armazenar os usuários com apenas Id e Nome
            List<UsuarioVM> listFun = new List<UsuarioVM>();

            // Obter apenas os campos Id e Nome da tabela TbUsuarios
            var listTb = _context.TbUsuarios
                                 .Select(u => new UsuarioVM
                                 {
                                     Id = u.Id,
                                     Nome = u.Nome
                                 })
                                 .ToList();

            // Retorna a lista já com os campos filtrados
            return listTb;
        }
    }
}
