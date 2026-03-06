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
        // General
        public const string Save = "Salvar";
        public const string Cancel = "Cancelar";
        public const string Delete = "Excluir";
        public const string Deactivate = "Desativar";
        public const string Edit = "Editar";
        public const string Create = "Criar";
        public const string Loading = "Carregando...";
        public const string Actions = "AÃ§Ãµes";
        public const string Details = "Detalhes";
        public const string Welcome = "Bem-vindo";
        public const string Logout = "Sair";
        public const string Login = "Entrar";
        public const string Search = "Pesquisar...";
        public const string Refresh = "Atualizar";
        public const string Back = "Voltar";
        public const string Generate = "Gerar";
        public const string Success = "Sucesso";
        public const string ConfirmDeactivate = "Tem certeza que deseja desativar este usuÃ¡rio?";

        // Navigation
        public const string Nav_Dashboard = "InÃ­cio";
        public const string Nav_Orders = "Ordens de ProduÃ§Ã£o";
        public const string Nav_Profile = "Meu Perfil";
        public const string Nav_Users = "UsuÃ¡rios";
        public const string Nav_Teams = "Equipes";
        public const string Nav_Reports = "RelatÃ³rios";
        public const string Nav_MyTasks = "Minhas Tarefas";
        public const string Nav_DelegateTasks = "Delegar Tarefas";
        public const string Nav_Settings = "Ajustes do Sistema";
        public const string Nav_TvMode = "Modo TV";

        // Dashboard
        public const string Dash_Title = "Dashboard de ProduÃ§Ã£o";
        public const string Dash_CompletionRate = "Taxa de ConclusÃ£o";
        public const string Dash_ActiveOrders = "Ordens Ativas";
        public const string Dash_CompletedToday = "ConcluÃ­das Hoje";
        public const string Dash_AvgLeadTime = "Tempo MÃ©dio (Lead)";
        public const string Dash_Efficiency = "EficiÃªncia";
        public const string Dash_WeeklyVolume = "Volume Semanal (Ãšltimos 7 Dias)";
        public const string Dash_WorkloadByOp = "Carga por Operador";
        public const string Dash_NormalOperation = "OperaÃ§Ã£o Normal";
        public const string Dash_FactoryFloor = "No chÃ£o de fÃ¡brica";
        public const string Dash_OrdersByStage = "Ordens por Etapa";
        public const string Dash_AvgHours = "MÃ©dia de horas";
        public const string Dash_Alerts = "Alertas / Paradas";
        public const string Dash_NoStopped = "Nenhuma ordem parada encontrada";
        public const string Dash_Workload = "Carga de Trabalho por UsuÃ¡rio";
        public const string Dash_NoActiveWork = "Sem atividade ativa";
        public const string Dash_TvMode = "Modo TV";
        public const string Dash_RecentActivity = "Atividades Recentes";
        public const string Dash_RealTime = "Tempo Real";
        public const string Dash_NoRecentActivity = "Nenhuma atividade recente.";

        // Production Orders
        public const string OP_Title = "Ordens de ProduÃ§Ã£o";
        public const string OP_NewOrder = "Nova Ordem";
        public const string OP_Code = "CÃ³digo";
        public const string OP_Product = "Produto";
        public const string OP_Qty = "Qtd";
        public const string OP_Stage = "Etapa";
        public const string OP_Status = "Status";
        public const string OP_Delivery = "Entrega";
        public const string OP_AssignedTo = "AtribuÃ­do a";
        public const string OP_Unassigned = "NÃ£o atribuÃ­do";
        public const string OP_ExportCSV = "Exportar CSV";
        public const string OP_ExportExcel = "Exportar Excel";
        public const string OP_DailyPDF = "PDF DiÃ¡rio";
        public const string OP_Report = "RelatÃ³rio PDF";
        public const string OP_NoOrdersFound = "Nenhuma ordem de produÃ§Ã£o encontrada.";
        public const string OP_NoOrdersMatch = "Nenhuma ordem corresponde aos seus critÃ©rios de busca.";
        public const string OP_Urgent = "Prioridade Alta (Urgente)";
        public const string OP_NoUrgentFound = "Nenhuma ordem urgente";
        public const string OP_BatchCount = "Lotes";

        // Order Create / Details
        public const string OP_Create_Title = "Criar Nova Ordem de ProduÃ§Ã£o";
        public const string OP_Details_Title = "Detalhes da Ordem de ProduÃ§Ã£o";
        public const string OP_InfoDetails = "InformaÃ§Ãµes Detalhadas";
        public const string OP_UniqueCode = "CÃ³digo Ãšnico (Ex: OP-2024-001)";
        public const string OP_ProductDesc = "DescriÃ§Ã£o do Produto";
        public const string OP_EstimatedDelivery = "Data de Entrega Estimada";
        public const string OP_CreationDate = "Data de CriaÃ§Ã£o";
        public const string OP_History = "HistÃ³rico de ProduÃ§Ã£o";
        public const string OP_AdvanceStage = "AvanÃ§ar Etapa";
        public const string OP_UpdateStatus = "Atualizar Status";
        public const string OP_ResumeProduction = "Retomar ProduÃ§Ã£o";
        public const string OP_StopProduction = "Parar ProduÃ§Ã£o";
        public const string OP_MarkCompleted = "Marcar como Finalizado";
        public const string OP_AssignTask = "Delegar Tarefa";
        public const string OP_Note = "ObservaÃ§Ã£o / Nota";
        public const string OP_WorkflowTip = "Dica de Fluxo";
        public const string OP_WorkflowDesc = "As ordens devem seguir o fluxo: Corte -> Costura -> RevisÃ£o -> Embalagem.";
        public const string OP_BackToList = "Voltar para Lista";
        public const string OP_OrderNotFound = "Ordem nÃ£o encontrada.";
        public const string OP_ChangeAssignment = "Alterar atribuiÃ§Ã£o...";
        public const string OP_Controls = "Controles de ProduÃ§Ã£o";
        public const string OP_ChangeStage = "Alterar EstÃ¡gio / Retrabalho";
        public const string OP_NewStage = "Novo EstÃ¡gio";
        public const string OP_ReworkReason = "Motivo / ObservaÃ§Ã£o";
        public const string OP_ReworkRequired = "ObrigatÃ³rio para retornar a uma fase anterior.";
        public const string OP_QuickActions = "Atalhos RÃ¡pidos";
        public const string OP_PrintTag = "Imprimir Ficha";
        public const string OP_FinancialAnalysis = "AnÃ¡lise Financeira";
        public const string OP_TotalCost = "Custo Total";
        public const string OP_CostPerPiece = "Custo / PeÃ§a";
        public const string OP_ProfitMargin = "Margem";
        public const string OP_DefectsFound = "Defeitos Registrados";
        public const string OP_NoDefects = "Nenhum defeito registrado nesta ordem.";
        public const string OP_ReportDefect = "Reportar Defeito";
        public const string OP_ConfirmFinalize = "Confirmar FinalizaÃ§Ã£o";
        public const string OP_FinalizeWarning = "Tem certeza que deseja finalizar a produÃ§Ã£o da ordem?";
        public const string OP_FinalizeEffects = "Esta aÃ§Ã£o irÃ¡ encerrar o cronÃ´metro, calcular custos e atualizar estoque.";

        // History Table
        public const string Hist_Date = "Data";
        public const string Hist_From = "De";
        public const string Hist_To = "Para";
        public const string Hist_User = "UsuÃ¡rio";
        public const string Hist_Note = "Nota";
        public const string Hist_Action = "AÃ§Ã£o";

        // User Management
        public const string User_Title = "Gerenciamento de UsuÃ¡rios";
        public const string User_NewUser = "Novo UsuÃ¡rio";
        public const string User_User = "UsuÃ¡rio";
        public const string User_Name = "Nome";
        public const string User_Email = "E-mail";
        public const string User_Role = "Perfil / FunÃ§Ã£o";
        public const string User_Status = "Status";
        public const string User_Active = "Ativo";
        public const string User_Inactive = "Inativo";
        public const string User_PublicId = "ID PÃºblico (UUID)";
        public const string User_Password = "Senha";
        public const string User_PassHint = "(Deixe em branco para manter a atual)";
        public const string User_GenerateUUID = "Gerar UUID";
        public const string User_UUIDRequired = "UUID PÃºblico Ã© obrigatÃ³rio.";

        // Profile
        public const string Prof_Title = "Meu Perfil";
        public const string Prof_ChangePass = "Alterar Senha";
        public const string Prof_CurrentPass = "Senha Atual";
        public const string Prof_NewPass = "Nova Senha";
        public const string Prof_ConfirmPass = "Confirmar Nova Senha";

        // Roles
        public const string Role_Admin = "Administrador";
        public const string Role_Leader = "LÃ­der";
        public const string Role_Operator = "Costureira";
        public const string Role_Workshop = "Oficina";

        // Stages
        public const string Stage_Cutting = "Corte";
        public const string Stage_Sewing = "Costura";
        public const string Stage_Review = "RevisÃ£o";
        public const string Stage_Packaging = "Embalagem";

        // Status
        public const string Status_InProduction = "Em ProduÃ§Ã£o";
        public const string Status_Stopped = "Parado";
        public const string Status_Completed = "Finalizado";
        public const string Status_Paused = "Pausado";
        public const string Status_Finished = "ConcluÃ­do";

        // Toasts / Messages
        public const string Msg_OrderCreated = "Ordem de produÃ§Ã£o criada";
        public const string Msg_OrderUpdated = "Ordem atualizada";
        public const string Msg_StatusUpdated = "Status atualizado";
        public const string Msg_StageAdvanced = "Etapa avanÃ§ada com sucesso";
        public const string Msg_TaskAssigned = "Tarefa delegada com sucesso";
        public const string Msg_UserCreated = "UsuÃ¡rio criado";
        public const string Msg_UserUpdated = "UsuÃ¡rio atualizado";
        public const string Msg_PassChanged = "Senha alterada com sucesso";
        public const string Msg_Error = "Ocorreu um erro";
        public const string Msg_LoginFailed = "Falha no login. Verifique suas credenciais.";

        // Catalog
        public const string Cat_Title = "CatÃ¡logo de Produtos";
        public const string Cat_NewProduct = "Novo Produto";
        public const string Cat_EditProduct = "Editar Produto";
        public const string Cat_MainSku = "SKU Principal";
        public const string Cat_InternalCode = "CÃ³digo Interno";
        public const string Cat_FabricType = "Tecido";
        public const string Cat_AvgTime = "Tempo MÃ©dio (min)";
        public const string Cat_Sizes = "Tamanhos";
        public const string Cat_NoProducts = "Nenhum produto encontrado.";
        public const string Cat_SearchHint = "Buscar por SKU ou Nome...";
        public const string Cat_EstSalePrice = "PreÃ§o Venda Estimado (R$)";
        public const string Cat_AvailSizes = "Tamanhos DisponÃ­veis";
        public const string Cat_AddSize = "Adicionar";
        public const string Cat_NoSizes = "Nenhum tamanho adicionado.";
        public const string Cat_SuccessCreated = "Produto criado com sucesso!";
        public const string Cat_SuccessUpdated = "Produto atualizado!";
        public const string Cat_SuccessDeleted = "Produto removido.";
        public const string Cat_ErrLoad = "Erro ao carregar catÃ¡logo.";
        public const string Cat_ErrDeleteLinked = "NÃ£o Ã© possÃ­vel excluir: produto possui ordens vinculadas.";
        public const string Cat_ConfirmDelete = "Tem certeza que deseja excluir este produto?";
        public const string Cat_NoLinkedProduct = "Sem produto vinculado";
        public const string Cat_UnknownItem = "Elemento Desconhecido";

        // Toasts
        public const string Toast_SystemNotice = "NotificaÃ§Ã£o do Sistema";
        public const string Toast_Close = "Fechar";
        public const string OP_Finalize = "Finalizar Ordem";
        public const string OP_MainInfo = "InformaÃ§Ãµes Principais";
        public const string OP_CurrentAssignment = "AtribuiÃ§Ã£o Atual";
        public const string OP_OperatorAssigned = "Operador AtribuÃ­do";
        public const string OP_AssignOperator = "Atribuir Operador";

        public const string QA_Title = "Qualidade";
        public const string QA_Reason = "Motivo";
        public const string QA_Photo = "Foto";
        public const string QA_NoDefects = "Nenhum defeito registrado.";

        public const string Product = "Produto";
        public const string Quantity = "Quantidade";
        public const string ExportPDF = "Exportar PDF";
        public const string OP_Action = "AÃ§Ã£o";
    }
}


