﻿module Fantomas.CodePrinter

open System
open Fantomas.SourceParser
open Fantomas.FormatConfig

let rec genParsedInput = function
    | ImplFile im -> genImpFile im
    | SigFile si -> genSigFile si

and genImpFile = function
    | ParsedImplFileInput(hs, mns) ->
        // Each module is separated by a number of blank lines
        col sepNln mns genModuleOrNamespace

and genSigFile si = failwith "Not implemented yet"

and genModuleOrNamespace = function
    | ModuleOrNamespace(ats, px, ao, li, mds) ->
        col sepNln mds genModuleDecl

and genModuleDecl = function
    | Attributes(ats) -> col sepArgs ats genAttribute
    | DoExpr(e) ->  genExpr e
    | Exception(ex) -> genException ex
    | HashDirective(s1, s2) -> !- "#" -- s1 +> sepSpace -- s2
    | Let(LetBinding(px, ats, ao, p, e)) -> !- "let " +> genPat p +> sepEq +> genExpr e
    | LetRec(bs) -> !- "[LetRec]"
    | ModuleAbbrev(s1, s2) -> !- "module " -- s1 +> sepEq -- s2
    | NamespaceFragment(m) -> !- "[NamespaceFragment]"
    | NestedModule(ats, px, ao, s, mds) -> 
        colOpt sepArgs sepNln ats genAttribute +> colOpt sepNln sepNln (genPreXmlDoc px) (!-)
        -- "module " +> opt sepSpace ao genAccess -- s +> sepEq
        +> incIndent +> sepNln
        +> col sepNln mds genModuleDecl
    | Open(s) -> !- (sprintf "open %s" s)
    | Types(sts) -> col sepNln sts genTypeDefn
    | md -> failwithf "Unexpected pattern: %O" md

and genAccess(Access s) = !- s

and genAttribute(Attribute(li, e, isGetSet)) = !- "[<" -- li +> genExpr e -- ">]"
    
and genPreXmlDoc(PreXmlDoc lines) = lines

and genExpr = function
    // Superfluous paren in tuple
    | SingleExpr(Paren, (Tuple es as e)) -> genExpr e
    | SingleExpr(Paren, e) -> !- "(" +> genExpr e -- ")"
    | SingleExpr(Do, e) -> !- "do " +> genExpr e
    | SingleExpr(kind, e) -> id
    | ConstExpr(Const s) -> !- s
    | NullExpr -> id
    | Quote(e1, e2) -> id
    | ConstExpr(Const s) -> !- s 
    | TypedExpr(_, e, t) -> id
    | Tuple(es) -> !- "(" +> col sepArgs es genExpr -- ")"
    | ArrayOrList(es) -> id
    | Record(xs) -> id
    | ObjExpr(t, x, bd, ims) -> id
    | While(e1, e2) -> id
    | For(s, e1, e2, e3) -> id
    | ForEach(p, e1, e2) -> id
    | CompExpr(isList, e) -> id
    | ArrayOrListOfSeqExpr(e) -> id
    | Lambda(e, cs) -> id
    | Match(e, cs) -> id
    | Sequential(e1, e) -> id
    | App(e1, e2) -> id
    | TypeApp(e, ts) -> id
    | LetOrUse(isRec, isUse, bs, e) -> id
    | TryWith(e, cs) -> id
    | TryFinally(e1, e2) -> id
    | Sequential(e1, e2) -> id
    | IfThenElse(e1, e2, e3) -> id
    | Var(li) -> !- li
    | LongIdentSet(e) -> id
    | DotIndexedGet(e, es) -> id
    | DotIndexedSet(e1, es, e2) -> id
    | DotGet(e, s) -> id
    | DotSet(e1,_s, e2) -> id
    | TraitCall(ss, msg, e) -> id
    | LetOrUseBang(isUse, p, e1, e2) -> id
    | e -> failwithf "Unexpected pattern: %O" e

and genException e = id

and genTypeDefn td = id

and genPat = function
    | PatOptionalVal(x) -> id
    | PatAttrib(p, attrs) -> id
    | PatOr(p1, p2) -> id
    | PatAnds(ps) -> id
    | PatNullary PatNull -> id
    | PatNullary PatWild -> id
    | PatTyped(p, t) -> id
    | PatNamed(ao, p, s) -> !- s
    | PatLongIdent(ao, li, ps) -> id
    | PatParen(p) -> id
    | PatSeq(PatTuple, ps) -> id
    | PatSeq(PatArray, ps) -> id
    | PatSeq(PatList, ps) -> id
    | PatRecord(xs) -> id
    | PatConst(Const s) -> !- s
    | PatIsInst(p) -> id
    | p -> failwithf "Unexpected pattern: %O" p
        
        



