var express = require('express');
var app = express();
var fs = require("fs");
var qs = require('querystring');
var Web3 = require('web3');
var solc = require('solc');
var nodemailer = require('nodemailer');
const MerkerBuild = require('../build/contracts/MerkeleRootSubmit.json');
const e = require('express');
var sha256 = require('js-sha256');


app.use(express.urlencoded());
app.use(express.json());

app.post('/merkleroot', function (req, res) {
    //request
    res.setHeader('Content-Type', 'application/json');
    var rootHash = req.body.roothash;
    console.log("Start storage merkle root with request: "+req.body);
    console.log('the roothash is: ' + rootHash);

    //deploy
    deployContract(rootHash, res);

})
app.post('/getRoot',function(req,res){
    res.setHeader('Content-Type', 'application/json');
    var contractAddress = req.body.address;
    getDataFromContractAddress(contractAddress,res);
})

app.post('/checkData',function (req, res){
    res.setHeader('Content-Type', 'application/json');
    let contractAddress = req.body.ContractAddress;
    // let merkleRoot = req.body.merkleRoot;
    let email = req.body.Email;
    let auxilaryPath = req.body.AuxiPath; //we need an array here!
    let fileName= req.body.FileFullName

    //test data
    console.log("Start check data intergrity with request: "+req.body);
    let rootCalculate = calculateRootHash(auxilaryPath);
    console.log("root hash calculate is: "+rootCalculate);


    //verify Data
    var web3 = new Web3(new Web3.providers.HttpProvider('http://host.docker.internal:7545'));
    const abi = MerkerBuild.abi;
    const Merkel = new web3.eth.Contract(abi, contractAddress);
    const get = async () => {
        try{
        const data = await Merkel.methods.verify(rootCalculate).call();
        return data;
        }catch (error){
            throw 'We got an error'+error;
        }
    }
    get().then(function (xx){
        console.log("Result of checking data intergrity of file "+fileName+" is "+xx);
        if(xx == true){
            sendEmail(fileName,email,true);
        } else {
            sendEmail(fileName,email,false);
        }
        var afterExecute = { "intergrity": xx };
        res.end(JSON.stringify(afterExecute));
    }).catch((error)=>{
        console.error(error);
        res.end(error);
    });

    // res.end();

    // sendEmail();
    // res.end();
});

var server = app.listen(3000, function () {
    var host = server.address().address
    var port = server.address().port
    console.log("Example app listening at http://%s:%s", host, port)
})

function printJson(obj, prefix) {
    console.log(prefix + '    ' + JSON.stringify(obj, null, 2));
}

function deployContract(hashRoot, resp) {
    //init data
    var web3 = new Web3(new Web3.providers.HttpProvider('http://host.docker.internal:7545'));
    const byteCode = MerkerBuild.bytecode;
    const abi = MerkerBuild.abi;
    const privKey = '0847277e7f9d2b4960b804fd9dfc2e43cf58db08edd96abaa073858526be30a0'; //private of any account! 
    const address = '0x46932985E37d068dF02d702e275f5e65191b6d81'//account address

    const deploy = async () => {
        console.log(`Attempting to deploy from account: ${address}`);

        const incrementer = new web3.eth.Contract(abi);

        const incrementerTx = incrementer.deploy({
            data: byteCode,
            arguments: [hashRoot],
        });

        const createTransaction = await web3.eth.accounts.signTransaction(
            {
                from: address,
                data: incrementerTx.encodeABI(),
                gas: await incrementerTx.estimateGas(),
            },
            privKey
        );

        const createReceipt = await web3.eth.sendSignedTransaction(
            createTransaction.rawTransaction
        );
        console.log(`Contract deployed at address ${createReceipt.contractAddress}`);
        return createReceipt.contractAddress;
    };
    deploy().then(function (xx) {
        var afterExecute = { "yourAddress": xx };
        resp.end(JSON.stringify(afterExecute));
    });

}

function getDataFromContractAddress(contractAddress,resp) {
    var web3 = new Web3(new Web3.providers.HttpProvider('http://host.docker.internal:7545'));
    const abi = MerkerBuild.abi;
    const Merkel = new web3.eth.Contract(abi, contractAddress);
    const get = async () => {
        console.log(`Making a call to contract at address ${contractAddress}`);
        const data = await Merkel.methods.getRoot().call();
        console.log(`The current root hash stored is: ${data}`);
        return data;
    }
    get().then(function (xx){
        var afterExecute = { "merkleRoot": xx };
        resp.end(JSON.stringify(afterExecute));
    });
}

function sendEmail(fileName,email, result){
    console.log("Start sending Email with with result: "+result);
    let content ='';
    if(result==true){
        content = `<!DOCTYPE html>
        <html>
        <head>
        <title>Page Title</title>
        </head>
        <body>
        
        <h1>Data intergrity</h1>
        <p>File name is: ${fileName}</p>
        <p style="color:green">Your data is INTERGRITY</p>
        
        </body>
        </html>`;
    } else {
        content = `<!DOCTYPE html>
        <html>
        <head>
        <title>Page Title</title>
        </head>
        <body>
        
        <h1>Data intergrity</h1>
        <p>File name is: ${fileName}</p>
        <p style="color:red">Your data is changed, please contact to CSS to verify it</p>
        
        </body>
        </html>`;
    }
    var transporter = nodemailer.createTransport({
        service: 'gmail',
        auth: {
          user: 'nhdoan.sdh20@hcmut.edu.vn',
          pass: 'moonrider'
        }
      });
      
      var mailOptions = {
        from: 'nhdoan.sdh20@hcmut.edu.vn',
        to: email,
        subject: 'VerifyData Result',
        html: content
      };
      
      transporter.sendMail(mailOptions, function(error, info){
        if (error) {
          console.log(error);
        } else {
          console.log('Email sent: ' + info.response);
        }
      });
}

function calculateRootHash(auxilaryPath){
    if(typeof myVar !== 'undefined' || auxilaryPath.length ==0) {
        console.log('NOT FOUND AUxilary path');
        return null;
    }
    let i;
    let rs;
    rs = sha256(auxilaryPath[0].Left + auxilaryPath[0].Right);
    for (i = 1; i < auxilaryPath.length; i++) {
        left = auxilaryPath[i].Left;
        right = auxilaryPath[i].Right;
        if(left ==='NULL' ){
            rs = sha256(rs+right);
        } else {
            rs = sha256(left+rs);
        }
    }
    return rs;
}
