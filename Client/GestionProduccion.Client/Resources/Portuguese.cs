/*
 * Copyright (c) 2026 David Fernandez Garzon. All rights reserved.
 * 
 * This software and its associated documentation files are the exclusive property 
 * of David Fernandez Garzon. Unauthorized copying, modification, distribution, 
 * or use of this software, via any medium, is strictly prohibited.
 * 
 * Proprietary and Confidential.
 */

namespace GestionProduccion.Client.Resources
{
    public static class Portuguese
    {
        // Geral
        public const string Save = "Salvar";
        public const string Cancel = "Cancelar";
        public const string Delete = "Excluir";
        public const string Deactivate = "Desativar";
        public const string Edit = "Editar";
        public const string Create = "Criar";
        public const string Loading = "Carregando...";
        public const string Actions = "Ações";
        public const string Details = "Detalhes";
        public const string Welcome = "Bem-vindo";
        public const string Logout = "Sair";
        public const string Login = "Entrar";
        public const string Search = "Pesquisar...";
        public const string Refresh = "Atualizar";
        public const string Back = "Voltar";
        public const string Generate = "Gerar";
        public const string Success = "Sucesso";
        public const string ConfirmDeactivate = "Tem certeza que deseja desativar este usuário?";
        public const string Filter = "Filtrar";
        public const string Selected = "selecionado(s)";
        public const string BulkActions = "Ações em Massa";
        public const string Pending = "Pendente";
        public const string Cancelled = "Cancelado";
        public const string Pause = "Pausar";
        public const string Resume = "Retomar";
        public const string Stop = "Parar";
        public const string Guest = "Convidado";
        public const string PleaseLogin = "Por favor, faça login";

        // Navegação
        public const string Nav_Dashboard = "Início";
        public const string Nav_Orders = "Ordens de Produção";
        public const string Nav_Profile = "Meu Perfil";
        public const string Nav_Users = "Usuários";
        public const string Nav_Teams = "Equipes";
        public const string Nav_Reports = "Relatórios";
        public const string Nav_MyTasks = "Minhas Tarefas";
        public const string Nav_DelegateTasks = "Delegar Tarefas";
        public const string Nav_Settings = "Configurações";
        public const string Nav_TvMode = "Modo TV";

        // Menus
        public const string Menu_Production = "PRODUÇÃO";
        public const string Menu_FactoryManagement = "GESTÃO DE FÁBRICA";
        public const string Menu_IntelligenceBI = "INTELIGÊNCIA & BI";
        public const string Menu_Settings = "CONFIGURAÇÕES";

        // Dashboard
        public const string Dash_Title = "Painel de Controle de Produção";
        public const string Dash_CompletionRate = "Taxa de Conclusão";
        public const string Dash_ActiveOrders = "Ordens Ativas";
        public const string Dash_CompletedToday = "Concluídas Hoje";
        public const string Dash_AvgLeadTime = "Tempo Médio de Ciclo";
        public const string Dash_Efficiency = "Eficiência Diária";
        public const string Dash_WeeklyVolume = "Volume Semanal (Últimos 7 Dias)";
        public const string Dash_WorkloadByOp = "Carga por Operador";
        public const string Dash_NormalOperation = "Operação Normal";
        public const string Dash_FactoryFloor = "No chão de fábrica";
        public const string Dash_OrdersByStage = "Ordens por Etapa";
        public const string Dash_AvgHours = "Média de horas";
        public const string Dash_Alerts = "Alertas / Paradas";
        public const string Dash_NoStopped = "Nenhuma ordem parada encontrada";
        public const string Dash_Workload = "Carga de Trabalho por Usuário";
        public const string Dash_NoActiveWork = "Sem atividade activa";
        public const string Dash_TvMode = "Modo TV";
        public const string Dash_RecentActivity = "Atividades Recentes";
        public const string Dash_RealTime = "Tempo Real";
        public const string Dash_NoRecentActivity = "Nenhuma atividade recente.";

        // Ordens de Produção (OP)
        public const string OP_Title = "Ordens de Produção";
        public const string OP_NewOrder = "Nova Ordem";
        public const string OP_Code = "Código";
        public const string OP_Product = "Produto";
        public const string OP_Qty = "Quantidade";
        public const string OP_Stage = "Etapa";
        public const string OP_Status = "Status";
        public const string OP_Delivery = "Entrega";
        public const string OP_AssignedTo = "Atribuído a";
        public const string OP_Unassigned = "Não atribuído";
        public const string OP_ExportCSV = "Exportar CSV";
        public const string OP_ExportExcel = "Exportar Excel";
        public const string OP_DailyPDF = "PDF Diário";
        public const string OP_Report = "Relatório PDF";
        public const string OP_NoOrdersFound = "Nenhuma ordem de produção encontrada.";
        public const string OP_NoOrdersMatch = "Nenhuma ordem corresponde aos critérios de busca.";
        public const string OP_Urgent = "Prioridade Alta (Urgente)";
        public const string OP_NoUrgentFound = "Nenhuma ordem urgente";
        public const string OP_BatchCount = "Lotes";
        public const string OP_SearchPlaceholder = "Produto ou Código...";
        public const string OP_ClientPlaceholder = "Cliente (Opcional)...";
        public const string OP_SizePlaceholder = "Tamanho...";

        // Criação / Detalhes de OP
        public const string OP_Create_Title = "Criar Nova Ordem de Produção";
        public const string OP_NewOrderTitle = "Nova Ordem de Produção";
        public const string OP_Details_Title = "Detalhes de Ordem de Produção";
        public const string OP_InfoDetails = "Informações Detalhadas";
        public const string OP_UniqueCode = "Código Único (Ex: OP-2024-001)";
        public const string OP_ProductDesc = "Descrição do Produto";
        public const string OP_EstimatedDelivery = "Previsão de Entrega";
        public const string OP_CreationDate = "Data de Criação";
        public const string OP_History = "Histórico de Produção";
        public const string OP_AdvanceStage = "Avançar Etapa";
        public const string OP_UpdateStatus = "Atualizar Status";
        public const string OP_ResumeProduction = "Retomar Produção";
        public const string OP_StopProduction = "Parar Produção";
        public const string OP_MarkCompleted = "Marcar como Concluído";
        public const string OP_AssignTask = "Delegar Tarefa";
        public const string OP_Note = "Observação / Nota";
        public const string OP_WorkflowTip = "Fluxo de Trabalho";
        public const string OP_WorkflowDesc = "As ordens devem seguir o fluxo: Corte -> Costura -> Revisão -> Embalagem.";
        public const string OP_BackToList = "Voltar para a Lista";
        public const string OP_OrderNotFound = "Ordem não encontrada.";
        public const string OP_ChangeAssignment = "Alterar atribuição...";
        public const string OP_Controls = "Controles de Produção";
        public const string OP_ChangeStage = "Alterar Etapa / Retrabalho";
        public const string OP_NewStage = "Nova Etapa";
        public const string OP_ReworkReason = "Motivo / Observação";
        public const string OP_ReworkRequired = "Obrigatório para retornar a una fase anterior.";
        public const string OP_QuickActions = "Ações Rápidas";
        public const string OP_PrintTag = "Imprimir Etiqueta";
        public const string OP_FinancialAnalysis = "Análise Financeira";
        public const string OP_TotalCost = "Custo Total";
        public const string OP_CostPerPiece = "Custo / Peça";
        public const string OP_ProfitMargin = "Margem de Lucro";
        public const string OP_DefectsFound = "Defeitos Registrados";
        public const string OP_NoDefects = "Nenhum defeito registrado nesta ordem.";
        public const string OP_ReportDefect = "Reportar Defeito";
        public const string OP_ConfirmFinalize = "Confirmar Finalização";
        public const string OP_FinalizeWarning = "Tem certeza que deseja finalizar a produção desta ordem?";
        public const string OP_FinalizeEffects = "Esta ação irá encerrar o cronômetro, calcular custos e actualizar estoque.";
        public const string OP_LoadingSelectors = "Carregando seletores dinâmicos...";
        public const string OP_AutoGeneration = "Geração Automática";
        public const string OP_AutoGenerationDesc = "O código do lote (OP) será gerado automaticamente pelo sistema após salvar.";
        public const string OP_SelectProduct = "Selecione um Produto...";
        public const string OP_AddSize = "Adicionar";
        public const string OP_Size = "Tamanho";
        public const string OP_ClientOptional = "Cliente (Opcional)";
        public const string OP_SewingTeamOptional = "Equipe de Costura (Opcional)";
        public const string OP_AssignTeamOptional = "Atribuir a uma equipe específica";
        public const string OP_AssignOperatorOptional = "Operador Específico (Opcional)";
        public const string OP_AssignOperatorOptionalPlaceholder = "Selecione um operador...";
        public const string OP_Saving = "Salvando...";
        public const string OP_SaveOrder = "Criar Ordem de Produção";
        public const string OP_OrderSummary = "Resumo da Ordem";
        public const string OP_GeneratedOnSave = "Gerado ao salvar";
        public const string OP_ProductionTip = "Dica de Produção";
        public const string OP_ProductionTipDesc = "Certifique-se de que a ficha técnica está anexada ao lote físico.";
        public const string OP_ErrValidationFieldsRed = "Por favor, corrija os erros destacados no formulário.";
        public const string OP_ErrLoadFormData = "Erro ao carregar dados do formulário.";
        public const string OP_ErrSelectSizes = "Adicione ao menos um tamanho e quantidade.";
        public const string OP_SuccessGenerated = "Ordem gerada com sucesso!";
        public const string OP_TotalBatchCost = "Custo Total do Lote";
        public const string OP_RealUnitCost = "Custo Real Unitário";
        public const string OP_PartialOutput = "Baixa Parcial";
        public const string OP_By = "por";
        public const string OP_NotAssignedYet = "Esta ordem ainda não foi atribuída a um operador.";
        public const string OP_AssignNow = "Atribuir Agora";
        public const string OP_DetailedProgress = "Progresso Detalhado";
        public const string OP_InAndamento = "Em andamento";
        public const string OP_SelectOperator = "Selecionar Operador";
        public const string OP_Select = "Selecionar...";
        public const string OP_Assign = "Atribuir";
        public const string OP_NewStageLabel = "Nova Etapa:";
        public const string OP_ReasonNote = "Observação/Motivo:";
        public const string OP_ChangeReasonPlaceholder = "Descreva o motivo da alteração...";
        public const string OP_ConfirmChange = "Confirmar Alteração";
        public const string OP_FinalizeOrder = "Finalizar Produção";
        public const string OP_ConfirmFinalizeOrder = "Tem certeza que deseja finalizar a produção da ordem";
        public const string OP_FinalizeActionWill = "Esta ação irá:";
        public const string OP_FinalizeStopTimer = "Encerrar o cronômetro de produção.";
        public const string OP_FinalizeCalcCosts = "Calcular custos reais finais.";
        public const string OP_FinalizeUpdateStock = "Atualizar o estoque de produtos prontos.";
        public const string OP_ConfirmAndFinalize = "Confirmar e Finalizar";
        public const string OP_PartialOutputStage = "Baixa Parcial - Etapa:";
        public const string OP_PartialOutputDesc = "Registre a quantidade de peças que foram concluídas neste estágio hoje. O sistema avançará o estágio automaticamente quando todas as peças forem concluídas.";
        public const string OP_TotalOrder = "Total Ordem";
        public const string OP_AlreadyCompleted = "Já Concluído";
        public const string OP_QtyReadyNow = "Qtd Pronta Agora";
        public const string OP_SaveProduction = "Salvar Produção";
        public const string OP_ErrMinQty = "A quantidade deve ser maior que zero.";
        public const string OP_SuccessPartialOutput = "Produção registrada!";
        public const string OP_GeneratingPdf = "Gerando PDF...";
        public const string OP_ErrLoadUsers = "Erro ao carregar usuários.";

        // Tabela de Histórico
        public const string Hist_User = "Usuário";
        public const string Hist_Note = "Nota";
        public const string Hist_Action = "Ação";

        // Gestão de Usuários
        public const string User_Title = "Gestão de Usuários";
        public const string User_NewUser = "Novo Usuário";
        public const string User_User = "Usuário";
        public const string User_Name = "Nome";
        public const string User_Email = "E-mail";
        public const string User_Role = "Perfil / Função";
        public const string User_Status = "Status";
        public const string User_Active = "Ativo";
        public const string User_Inactive = "Inativo";
        public const string User_PublicId = "ID Público (UUID)";
        public const string User_Password = "Senha";
        public const string User_PassHint = "(Deixe em branco para manter a senha atual)";
        public const string User_GenerateUUID = "Gerar UUID";
        public const string User_UUIDRequired = "UUID Público é obrigatório.";
        public const string User_FullNamePlaceholder = "Nome completo do funcionário";
        public const string User_EmailPlaceholder = "exemplo@empresa.com";

        // Perfil
        public const string Prof_Title = "Meu Perfil";
        public const string Prof_ChangePass = "Alterar Senha";
        public const string Prof_CurrentPass = "Senha Atual";
        public const string Prof_NewPass = "Nova Senha";
        public const string Prof_ConfirmPass = "Confirmar Nova Senha";

        // Cargos / Funções
        public const string Role_Admin = "Administrador";
        public const string Role_Leader = "Líder de Produção";
        public const string Role_Operator = "Operador(a) / Costureiro(a)";
        public const string Role_Workshop = "Oficina / Terceirizado";
        public const string Role_Office = "Escritório / Administrativo";

        // Etapas
        public const string Stage_Cutting = "Corte";
        public const string Stage_Sewing = "Costura";
        public const string Stage_Review = "Revisão";
        public const string Stage_Packaging = "Embalagem";

        // Status
        public const string Status_InProduction = "Em Produção";
        public const string Status_Stopped = "Interrompido";
        public const string Status_Completed = "Concluído";
        public const string Status_Paused = "Pausado";
        public const string Status_Finished = "Finalizado";

        // Mensagens / Toasts
        public const string Msg_OrderCreated = "Ordem de produção criada con sucesso.";
        public const string Msg_OrderUpdated = "Ordem de produção atualizada.";
        public const string Msg_StatusUpdated = "Status de produção actualizado.";
        public const string Msg_StageAdvanced = "Etapa avançada com sucesso.";
        public const string Msg_TaskAssigned = "Tarefa delegada com sucesso.";
        public const string Msg_UserCreated = "Usuário cadastrado com sucesso.";
        public const string Msg_UserUpdated = "Dados do usuário atualizados.";
        public const string Msg_PassChanged = "Senha alterada com sucesso.";
        public const string Msg_Error = "Ocorreu un erro inesperado";
        public const string Msg_LoginFailed = "Falha no login. Verifique suas credenciais.";
        public const string Msg_SystemInitializing = "Inicializando sistema...";
        public const string Msg_SystemChecking = "Verificando sistema...";

        // Catálogo
        public const string Cat_Title = "Catálogo de Produtos";
        public const string Cat_NewProduct = "Novo Produto";
        public const string Cat_EditProduct = "Editar Produto";
        public const string Cat_MainSku = "SKU Principal";
        public const string Cat_InternalCode = "Código Interno";
        public const string Cat_FabricType = "Tipo de Tecido";
        public const string Cat_AvgTime = "Tempo Médio Estimado (min)";
        public const string Cat_Sizes = "Grade de Tamanhos";
        public const string Cat_NoProducts = "Nenhum produto no catálogo.";
        public const string Cat_SearchHint = "Buscar por SKU ou Nome...";
        public const string Cat_EstSalePrice = "Preço de Venda Estimado (R$)";
        public const string Cat_AvailSizes = "Tamanhos Disponíveis";
        public const string Cat_AddSize = "Adicionar";
        public const string Cat_NoSizes = "Nenhum tamanho configurado.";
        public const string Cat_SuccessCreated = "Produto cadastrado com sucesso!";
        public const string Cat_SuccessUpdated = "Dados do produto atualizados!";
        public const string Cat_SuccessDeleted = "Produto removido do catálogo.";
        public const string Cat_ErrLoad = "Erro ao carregar o catálogo de produtos.";
        public const string Cat_ErrDeleteLinked = "Não é possível excluir: existem ordens vinculadas a este produto.";
        public const string Cat_ConfirmDelete = "Tem certeza que deseja remover este produto definitivamente?";
        public const string Cat_NoLinkedProduct = "Nenhum produto vinculado";
        public const string Cat_UnknownItem = "Item Desconhecido";
        public const string Cat_Price = "Preço";
        public const string Cat_AvgTimeMinutes = "Tempo Médio (Min)";
        public const string Cat_EstPrice = "Preço Estimado";
        public const string Cat_SaveProduct = "Salvar Produto";
        public const string Cat_ErrValidationFields = "Por favor, preencha todos os campos obrigatórios.";
        public const string Cat_ErrValidation = "Erro de Validação";
        public const string Cat_ErrDuplicateCode = "Já existe un produto com este código SKU.";
        public const string Cat_ErrServerInaccessible = "Servidor inacessível.";
        public const string Cat_ErrDeleteActiveOrders = "Não é possível excluir: existem ordens vinculadas.";
        public const string Cat_ConflictData = "Conflito de Dados";

        // Configurações
        public const string Set_Title = "Configurações do Sistema";
        public const string Set_CompanyInfo = "Informações da Empresa";
        public const string Set_CompanyName = "Nome da Empresa";
        public const string Set_CompanyTaxId = "CNPJ / CPF";
        public const string Set_VisualIdentity = "Identidade Visual (Logo)";
        public const string Set_RemoveLogo = "Remover Logo";
        public const string Set_NoLogo = "Nenhum logotipo carregado.";
        public const string Set_UploadLogoHint = "Carregar novo logotipo (PNG o JPG, máx. 1MB)";
        public const string Set_ThemeCustomization = "Personalização (Tema)";
        public const string Set_ThemeChoice = "Escolha o Tema:";
        public const string Set_FinancialParams = "Parâmetros Financeiros";
        public const string Set_DailyFixedCost = "Custo Fixo Diário (R$)";
        public const string Set_DailyFixedCostHint = "Soma de aluguel, luz, etc. dividido por dias úteis.";
        public const string Set_HourlyCost = "Custo Hora Operador (R$)";
        public const string Set_HourlyCostHint = "Valor médio pago por hora aos operários.";
        public const string Set_TvMode = "Modo TV";
        public const string Set_TvAnnouncement = "Aviso do Modo TV";
        public const string Set_TvAnnouncementPlaceholder = "Ex: Reunião às 15h no setor de corte...";
        public const string Set_TvAnnouncementHint = "Este texto será exibido alternadamente na tela do Modo TV.";
        public const string Set_SaveSettings = "Salvar Configurações";
        public const string Set_Important = "IMPORTANTE";
        public const string Set_ReportHint = "Essas informações serão utilizadas no cabeçalho de todos os relatórios PDF.";
        public const string Set_LogoQualityHint = "Recomendado: fundo transparente e proporção horizontal.";
        public const string Set_ErrLoad = "Erro ao carregar configurações.";
        public const string Set_ErrFileTooLarge = "O arquivo é muito grande. Máximo 1MB.";
        public const string Set_ErrReadImage = "Erro ao ler arquivo de imagem.";
        public const string Set_SuccessSave = "Configurações salvas com sucesso!";
        public const string Set_ErrSave = "Erro ao salvar configurações.";

        // Tarefas Admin
        public const string Task_DelegateAdmin = "Delegar Tarefas Administrativas";
        public const string Task_NewTask = "Nova Tarefa";
        public const string Task_TitleLabel = "Título da Tarefa";
        public const string Task_Description = "Descrição";
        public const string Task_DescriptionPlaceholder = "O que precisa ser feito?";
        public const string Task_Responsible = "Responsável";
        public const string Task_SelectUser = "Selecione um usuário...";
        public const string Task_Deadline = "Prazo de Entrega";
        public const string Task_RecentDelegated = "Tarefas Delegadas Recentemente";
        public const string Task_Task = "Tarefa";
        public const string Task_DeadlineShort = "Prazo";
        public const string Task_ErrLoadData = "Erro ao carregar dados de tarefas.";
        public const string Task_ErrSelectResponsible = "Selecione um responsável.";
        public const string Task_ErrCreate = "Erro ao criar tarefa.";
        public const string Task_InProgress = "Em andamento";
        public const string Task_Completed = "Concluída";

        // Toasts e Diversos
        public const string Toast_SystemNotice = "Notificação do Sistema";
        public const string Toast_Close = "Fechar";
        public const string OP_Finalize = "Finalizar Ordem";
        public const string OP_MainInfo = "Informações Principais";
        public const string OP_CurrentAssignment = "Atribuição Atual";
        public const string OP_OperatorAssigned = "Operador Responsável";
        public const string OP_AssignOperator = "Atribuir Responsável";

        public const string QA_Title = "Controle de Qualidade";
        public const string QA_Reason = "Motivo do Defeito";
        public const string QA_Photo = "Foto de Evidência";
        public const string QA_NoDefects = "Nenhum defeito registrado nesta etapa.";

        public const string Product = "Produto";
        public const string Quantity = "Quantidade";
        public const string ExportPDF = "Exportar PDF";
        public const string OP_Action = "Ação";

        public const string Team_Title = "Gestão de Equipes";
        public const string Team_NewTeam = "Nova Equipe";
        public const string Team_NoTeams = "Nenhuma equipe cadastrada.";
        public const string Team_Members = "Membros";
        public const string Team_ReassignmentBlock = "Bloqueio de Reatribuição";
        public const string Team_ErrNoEligibleUsers = "Não é possível criar equipes. Cadastre usuários com cargo Líder ou Operacional primeiro.";
        public const string Team_AddTeam = "Adicionar Equipe";
        public const string Team_NoTeamsFound = "Nenhuma equipe encontrada.";
        public const string Team_Name = "Nome da Equipe";
        public const string Team_EditTeam = "Editar Equipe";
        public const string Team_SelectMemberHint = "Selecione os membros da equipe:";
        public const string Team_ManageMembers = "Gerenciar Membros";
        public const string Team_ActiveTeam = "Equipe Ativa";
        public const string Team_SaveTeam = "Salvar Equipe";
        public const string Team_SuccessMembersUpdated = "Membros atualizados!";
        public const string Team_SuccessSaved = "Equipe salva com sucesso!";
        public const string Team_ErrSave = "Erro ao salvar equipe";
        public const string Team_ErrBusiness = "Erro de Negócio";
        public const string Team_ConfirmDelete = "ATENÇÃO: Ao excluir esta equipe, todos os seus membros serão transferidos automaticamente. Deseja continuar?";
        public const string Team_SuccessDeleted = "Equipe removida com sucesso.";
        public const string Team_ErrNoOtherTeams = "Erro: Não há outras equipes para receber os funcionários.";
        public const string Team_ErrLoadData = "Erro ao carregar equipes.";
    }
}
