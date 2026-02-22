namespace GestionProduccion.Client.Resources
{
    public static class Portuguese
    {
        // General
        public const string Save = "Salvar";
        public const string Cancel = "Cancelar";
        public const string Delete = "Deletar";
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
        
        // Navigation
        public const string Nav_Dashboard = "Dashboard";
        public const string Nav_Orders = "Ordens de Produção";
        public const string Nav_Profile = "Meu Perfil";
        public const string Nav_Users = "Usuários";
        
        // Dashboard
        public const string Dash_Title = "Dashboard de Produção";
        public const string Dash_CompletionRate = "Taxa de Conclusão";
        public const string Dash_ActiveOrders = "Ordens Ativas";
        public const string Dash_CompletedToday = "Concluídas Hoje";
        public const string Dash_AvgLeadTime = "Tempo Médio (Lead)";
        public const string Dash_Efficiency = "Eficiência";
        public const string Dash_WeeklyVolume = "Volume Semanal (Últimos 7 Dias)";
        public const string Dash_WorkloadByOp = "Carga por Operador";
        public const string Dash_NormalOperation = "Operação Normal";
        public const string Dash_FactoryFloor = "No chão de fábrica";
        public const string Dash_OrdersByStage = "Ordens por Etapa";
        public const string Dash_AvgHours = "Média de horas";
        public const string Dash_Alerts = "Alertas / Paradas";
        public const string Dash_NoStopped = "Nenhuma ordem parada encontrada";
        public const string Dash_Workload = "Carga de Trabalho por Usuário";
        public const string Dash_NoActiveWork = "Sem atividade ativa";
        public const string Dash_TvMode = "Modo TV";
        public const string Dash_RecentActivity = "Atividades Recentes";
        public const string Dash_RealTime = "Tempo Real";
        public const string Dash_NoRecentActivity = "Nenhuma atividade recente.";
        
        // Production Orders
        public const string OP_Title = "Ordens de Produção";
        public const string OP_NewOrder = "Nova Ordem";
        public const string OP_Code = "Código";
        public const string OP_Product = "Produto";
        public const string OP_Qty = "Qtd";
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
        public const string OP_NoOrdersMatch = "Nenhuma ordem corresponde aos seus critérios de busca.";
        public const string OP_Urgent = "Prioridade Alta (Urgente)";
        public const string OP_NoUrgentFound = "Nenhuma ordem urgente";
        public const string OP_BatchCount = "Lotes";
        
        // Order Create / Details
        public const string OP_Create_Title = "Criar Nova Ordem de Produção";
        public const string OP_Details_Title = "Detalhes da Ordem de Produção";
        public const string OP_InfoDetails = "Informações Detalhadas";
        public const string OP_UniqueCode = "Código Único (Ex: OP-2024-001)";
        public const string OP_ProductDesc = "Descrição do Produto";
        public const string OP_EstimatedDelivery = "Data de Entrega Estimada";
        public const string OP_CreationDate = "Data de Criação";
        public const string OP_History = "Histórico de Produção";
        public const string OP_AdvanceStage = "Avançar Etapa";
        public const string OP_UpdateStatus = "Atualizar Status";
        public const string OP_ResumeProduction = "Retomar Produção";
        public const string OP_StopProduction = "Parar Produção";
        public const string OP_MarkCompleted = "Marcar como Finalizado";
        public const string OP_AssignTask = "Delegar Tarefa";
        public const string OP_Note = "Observación / Nota";
        public const string OP_WorkflowTip = "Dica de Fluxo";
        public const string OP_WorkflowDesc = "As ordens devem seguir o fluxo: Corte -> Costura -> Revisão -> Embalagem.";
        public const string OP_BackToList = "Voltar para Lista";
        public const string OP_OrderNotFound = "Ordem não encontrada.";
        public const string OP_ChangeAssignment = "Alterar atribuição...";
        public const string OP_Controls = "Controles de Produção";
        public const string OP_ChangeStage = "Alterar Estágio / Retrabalho";
        public const string OP_NewStage = "Novo Estágio";
        public const string OP_ReworkReason = "Motivo / Observação";
        public const string OP_ReworkRequired = "Obrigatório para retornar a uma fase anterior.";
        public const string OP_QuickActions = "Atalhos Rápidos";
        public const string OP_PrintTag = "Imprimir Ficha";
        public const string OP_FinancialAnalysis = "Análise Financeira";
        public const string OP_TotalCost = "Custo Total";
        public const string OP_CostPerPiece = "Custo / Peça";
        public const string OP_ProfitMargin = "Margem";
        public const string OP_DefectsFound = "Defeitos Registrados";
        public const string OP_NoDefects = "Nenhum defeito registrado nesta ordem.";
        public const string OP_ReportDefect = "Reportar Defeito";
        public const string OP_ConfirmFinalize = "Confirmar Finalização";
        public const string OP_FinalizeWarning = "Tem certeza que deseja finalizar a produção da ordem?";
        public const string OP_FinalizeEffects = "Esta ação irá encerrar o cronômetro, calcular custos e atualizar estoque.";
        
        // History Table
        public const string Hist_Date = "Data";
        public const string Hist_From = "De";
        public const string Hist_To = "Para";
        public const string Hist_User = "Usuário";
        public const string Hist_Note = "Nota";
        public const string Hist_Action = "Ação";

        // User Management
        public const string User_Title = "Gerenciamento de Usuários";
        public const string User_NewUser = "Novo Usuário";
        public const string User_User = "Usuário";
        public const string User_Name = "Nome";
        public const string User_Email = "E-mail";
        public const string User_Role = "Perfil / Función";
        public const string User_Status = "Status";
        public const string User_Active = "Ativo";
        public const string User_Inactive = "Inativo";
        public const string User_PublicId = "ID Público (UUID)";
        public const string User_Password = "Senha";
        public const string User_PassHint = "(Deixe em branco para manter a atual)";
        public const string User_GenerateUUID = "Gerar UUID";
        public const string User_UUIDRequired = "UUID Público é obrigatório.";
        
        // Profile
        public const string Prof_Title = "Meu Perfil";
        public const string Prof_ChangePass = "Alterar Senha";
        public const string Prof_CurrentPass = "Senha Atual";
        public const string Prof_NewPass = "Nova Senha";
        public const string Prof_ConfirmPass = "Confirmar Nova Senha";
        
        // Roles
        public const string Role_Admin = "Administrador";
        public const string Role_Leader = "Líder";
        public const string Role_Operator = "Costureira";
        public const string Role_Workshop = "Oficina";
        
        // Stages
        public const string Stage_Cutting = "Corte";
        public const string Stage_Sewing = "Costura";
        public const string Stage_Review = "Revisão";
        public const string Stage_Packaging = "Embalagem";
        
        // Status
        public const string Status_InProduction = "Em Produção";
        public const string Status_Stopped = "Parado";
        public const string Status_Completed = "Finalizado";
        public const string Status_Paused = "Pausado";
        public const string Status_Finished = "Concluído";
        
        // Toasts / Messages
        public const string Msg_OrderCreated = "Ordem de produção criada";
        public const string Msg_OrderUpdated = "Ordem atualizada";
        public const string Msg_StatusUpdated = "Status atualizado";
        public const string Msg_StageAdvanced = "Etapa avançada com sucesso";
        public const string Msg_TaskAssigned = "Tarefa delegada com sucesso";
        public const string Msg_UserCreated = "Usuário criado";
        public const string Msg_UserUpdated = "Usuário atualizado";
        public const string Msg_PassChanged = "Senha alterada com sucesso";
        public const string Msg_Error = "Ocorreu um erro";
        public const string Msg_LoginFailed = "Falha no login. Verifique suas credenciais.";

        // Catalog
        public const string Cat_Title = "Catálogo de Produtos";
        public const string Cat_NewProduct = "Novo Produto";
        public const string Cat_EditProduct = "Editar Produto";
        public const string Cat_MainSku = "SKU Principal";
        public const string Cat_InternalCode = "Código Interno";
        public const string Cat_FabricType = "Tecido";
        public const string Cat_AvgTime = "Tempo Médio (min)";
        public const string Cat_Sizes = "Tamanhos";
        public const string Cat_NoProducts = "Nenhum produto encontrado.";
        public const string Cat_SearchHint = "Buscar por SKU ou Nome...";
        public const string Cat_EstSalePrice = "Preço Venda Estimado (R$)";
        public const string Cat_AvailSizes = "Tamanhos Disponíveis";
        public const string Cat_AddSize = "Adicionar";
        public const string Cat_NoSizes = "Nenhum tamanho adicionado.";
        public const string Cat_SuccessCreated = "Produto criado com sucesso!";
        public const string Cat_SuccessUpdated = "Produto atualizado!";
        public const string Cat_SuccessDeleted = "Produto removido.";
        public const string Cat_ErrLoad = "Erro ao carregar catálogo.";
        public const string Cat_ErrDeleteLinked = "Não é possível excluir: produto possui ordens vinculadas.";
        public const string Cat_ConfirmDelete = "Tem certeza que deseja excluir este produto?";
        public const string Cat_NoLinkedProduct = "Sem produto vinculado";
        public const string Cat_UnknownItem = "Elemento Desconcedido";

        // Toasts
        public const string Toast_SystemNotice = "Notificação do Sistema";
        public const string Toast_Close = "Fechar";
    }
}
