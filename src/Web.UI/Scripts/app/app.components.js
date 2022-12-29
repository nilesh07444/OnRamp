
/**
 * *******************************CUSTOM DOCUMENT*******************************************************
 * @param {any} params
 */

var PolicyViewModel = function (params) {
    //params.IsConditionalLogic = ko.observable(false);
    var self = this;
    self.data= (params);
    console.log('PolicyViewModel',params);
    
}
ko.components.register('Policy-component', {
    viewModel: PolicyViewModel,
    template: {
        element: 'policy-display-Component'
    },
});

var MemoViewModel = function (params) {
    var self = this;
    self.data=(params);
    console.log('MemoViewModel',params);
}
ko.components.register('Memo-component', {
    viewModel: MemoViewModel ,
    template: {
        element: 'memo-display-Component'
    },
});

var TMViewModel = function (params) {
    //params.IsConditionalLogic = ko.observable(false);
    var self = this;
    self.data = (params);
    console.log('TMViewModel', params);
    

}
ko.components.register('TM-component', {
    viewModel: TMViewModel,
    template: {
        element: 'TM-display-Component'
    },
});

var TestViewModel = function (params) {
    
    var self = this;
    
    self.data = (params);
    
    console.log('TestViewModel', params);
    
}
ko.components.register('Test-component', {
    viewModel:TestViewModel ,
    template: {
        element: 'Test-display-Component'
    },
});

var FormViewModel = function (params) {
    
    var self = this;
    
    self.data = (params);
    
    console.log('FormViewModel', params);
    
}
ko.components.register('Form-component', {
    viewModel: FormViewModel,
    template: {
        element: 'Form-display-Component'
    },
});

var CLViewModel = function (params) {
    
    var self = this;
    self.data = (params);
    console.log('CLViewModel', params);
    
}
ko.components.register('CL-component', {
    viewModel:CLViewModel ,
    template: {
        element: 'CL-display-Component'
    },
});

var AcrobatViewModel = function (params) {
    var self = this;
    self.data = (params);
    console.log('AcrobatViewModel',params);
}
ko.components.register('Acrobat-component', {
    viewModel: AcrobatViewModel,
    template: {
        element: 'Acrobat-display-Component'
    }
});

var PolicyPreviewViewModel = function (params) {
    var self = this;
    self.data = (params);
    console.log('PolicyPreviewViewModel', params);

}
ko.components.register('Policy-preview-component', {
    viewModel: PolicyPreviewViewModel,

    template: {
        element: 'policy-display-preview-component'
    },
});

var MemoPreviewViewModel = function (params) {
    var self = this;
    self.data = (params);
    console.log('MemoPreviewViewModel', params);
}
ko.components.register('Memo-preview-component', {
    viewModel: MemoPreviewViewModel,
    template: {
        element: 'memo-display-preview-component'
    },
});

var TMPreviewViewModel = function (params) {
    var self = this;
    self.data = (params);
    console.log('TMPreviewViewModel', params);
}
ko.components.register('TM-preview-component', {
    viewModel: TMPreviewViewModel,
    template: {
        element: 'TM-display-preview-component'
    },
});

var FormPreviewViewModel = function (params) {
    var self = this;

    self.data = (params);
    console.log('FormPreviewViewModel', params);
}
ko.components.register('Form-preview-component', {
    viewModel: FormPreviewViewModel,
    template: {
        element: 'Form-display-preview-component'
    },
});

var TestPreviewViewModel = function (params) {
    var self = this;

    self.data = (params);
    console.log('TestPreviewViewModel', params);
}
ko.components.register('Test-preview-component', {
    viewModel: TestPreviewViewModel,
    template: {
        element: 'Test-display-preview-component'
    },
});

var CLPreviewViewModel = function (params) {
    var self = this;
    self.data = (params);
    console.log('CLPreviewViewModel', params);
}
ko.components.register('CL-preview-component', {
    viewModel: CLPreviewViewModel,
    template: {
        element: 'CL-display-preview-component'
    },
});

var AcrobatPreviewViewModel = function (params) {
    var self = this;
    self.data = (params);
    console.log('AcrobatPreviewViewModel', params);
}
ko.components.register('Acrobat-preview-component', {
    viewModel: AcrobatPreviewViewModel,
    template: {
        element: 'Acrobat-display-preview-component'
    }
});




/*------------------------********** Sub Preview Option *************-----------------------*/
//var PolicyPreviewViewModel = function (parameters) {
//    var self = this;
//    self.data = (parameters);
//    console.log('PolicyPreviewViewModel', parameters);

//}
//ko.components.register('Policy-preview-component', {
//    viewModel: PolicyPreviewViewModel,

//    template: {
//        element: 'policy-display-preview-component'
//    },
//});

//var MemoPreviewViewModel = function (parameters) {
//    var self = this;
//    self.data = (parameters);
//    console.log('MemoPreviewViewModel', parameters);
//}

//ko.components.register('Memo-preview-component', {
//    viewModel: MemoPreviewViewModel,
//    template: {
//        element: 'memo-display-preview-component'
//    },
//});


//var TMPreviewViewModel = function (parameters) {
//    var self = this;
//    self.data = (parameters);
//    console.log('TMPreviewViewModel', parameters);
//}
//ko.components.register('TM-preview-component', {
//    viewModel: TMPreviewViewModel,
//    template: {
//        element: 'TM-display-preview-component'
//    },
//});



//var TestPreviewViewModel = function (parameters) {
//    var self = this;

//    self.data = (parameters);
//    console.log('TestPreviewViewModel', parameters);
//}

//ko.components.register('Test-preview-component', {
//    viewModel: TestPreviewViewModel,
//    template: {
//        element: 'Test-display-preview-component'
//    },
//});

//var CLPreviewViewModel = function (parameters) {
//    var self = this;
//    self.data = (parameters);
//    console.log('CLPreviewViewModel', parameters);
//}
//ko.components.register('CL-preview-component', {
//    viewModel: CLPreviewViewModel,
//    template: {
//        element: 'CL-display-preview-component'
//    },
//});


//var AcrobatPreviewViewModel = function (parameters) {
//    var self = this;
//    self.data = (parameters);
//    console.log('AcrobatPreviewViewModel', parameters);
//}
//ko.components.register('Acrobat-preview-component', {
//    viewModel: AcrobatPreviewViewModel,
//    template: {
//        element: 'Acrobat-display-preview-component'
//    }
//});
/*****************************************************************************************************************/





