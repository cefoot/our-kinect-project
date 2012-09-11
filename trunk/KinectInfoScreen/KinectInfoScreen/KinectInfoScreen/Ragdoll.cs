using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace KinectInfoScreen
{
    public class Ragdoll
    {
        private const float ArmDensity = 10;
        private const float LegDensity = 15;
        private const float LimbAngularDamping = 7;

        private Body _body;
        private Sprite _face;
        private Body _head;
        private Sprite _lowerArm;

        private Body _lowerLeftArm;
        private Body _leftHand;
        private Body _lowerLeftLeg;
        private Body _leftFoot;
        private Sprite _lowerLeg;
        private Body _lowerRightArm;
        private Body _rightHand;
        private Body _lowerRightLeg;
        private Body _rightFoot;
        private PhysicsGameScreen _screen;
        private Sprite _torso;
        private Sprite _upperArm;
        private Sprite _hand;
        private Sprite _foot;

        private Body _upperLeftArm;
        private Body _upperLeftLeg;
        private Sprite _upperLeg;
        private Body _upperRightArm;
        private Body _upperRightLeg;
        private Category _collisionCategories;
        private Category _collidesWith;
        private List<Body> _bodyParts = new List<Body>();

        public Ragdoll(World world, PhysicsGameScreen screen, Vector2 position)
        {
            CreateBody(world, position);
            CreateJoints(world);

            _screen = screen;
            CreateGFX();
        }

        public Body Body
        {
            get { return _body; }
        }

        public Body LeftFoot
        {
            get { return _leftFoot; }
        }

        public Body RightFoot
        {
            get { return _rightFoot; }
        }

        public Body RightHand
        {
            get { return _rightHand; }
        }

        public Body LeftHand
        {
            get { return _leftHand; }
        }

        public Body Head
        {
            get { return _head; }
        }

        public Category CollisionCategories
        {
            get {
                return _collisionCategories;
            }
            set {
                _collisionCategories = value;
                foreach (var bodyPart in _bodyParts)
                {
                    bodyPart.CollisionCategories = value;
                }
            }
        }

        public Category CollidesWith
        {
            get {
                return _collidesWith;
            }
            set {
                _collidesWith = value;
                foreach (var bodyPart in _bodyParts)
                {
                    bodyPart.CollidesWith = value;
                }
            }
        }

        //Torso
        private void CreateBody(World world, Vector2 position)
        {

            var mass = 2f;

            //Head
            _head = BodyFactory.CreateCircle(world, 0.9f, 10f);
            _bodyParts.Add(_head);
            _head.BodyType = BodyType.Dynamic;
            _head.AngularDamping = LimbAngularDamping;
            _head.Mass = mass;
            _head.Position = position;

            //Body
            _body = BodyFactory.CreateRoundedRectangle(world, 2f, 4f, 0.5f, 0.7f, 2, 10f);
            _bodyParts.Add(_body);
            _body.BodyType = BodyType.Dynamic;
            _body.Mass = mass;
            _body.Position = position + new Vector2(0f, 3f);

            //Left Arm
            _lowerLeftArm = BodyFactory.CreateCapsule(world, 1f, 0.45f, ArmDensity);
            _bodyParts.Add(_lowerLeftArm);
            _lowerLeftArm.BodyType = BodyType.Dynamic;
            _lowerLeftArm.AngularDamping = LimbAngularDamping;
            _lowerLeftArm.Mass = mass;
            _lowerLeftArm.Rotation = 1.4f;
            _lowerLeftArm.Position = position + new Vector2(-4f, 2.2f);

            _upperLeftArm = BodyFactory.CreateCapsule(world, 1f, 0.45f, ArmDensity);
            _bodyParts.Add(_upperLeftArm);
            _upperLeftArm.BodyType = BodyType.Dynamic;
            _upperLeftArm.AngularDamping = LimbAngularDamping;
            _upperLeftArm.Mass = mass;
            _upperLeftArm.Rotation = 1.4f;
            _upperLeftArm.Position = position + new Vector2(-2f, 1.8f);

            _leftHand = BodyFactory.CreateCapsule(world, 0.3f, 0.3f, ArmDensity);
            _bodyParts.Add(_leftHand);
            _leftHand.BodyType = BodyType.Dynamic;
            _leftHand.AngularDamping = LimbAngularDamping;
            _leftHand.Mass = mass;
            _leftHand.Rotation = 1.4f;
            _leftHand.Position = position + new Vector2(-4f, 2.7f);

            //Right Arm
            _lowerRightArm = BodyFactory.CreateCapsule(world, 1f, 0.45f, ArmDensity);
            _bodyParts.Add(_lowerRightArm);
            _lowerRightArm.BodyType = BodyType.Dynamic;
            _lowerRightArm.AngularDamping = LimbAngularDamping;
            _lowerRightArm.Mass = mass;
            _lowerRightArm.Rotation = -1.4f;
            _lowerRightArm.Position = position + new Vector2(4f, 2.2f);

            _upperRightArm = BodyFactory.CreateCapsule(world, 1f, 0.45f, ArmDensity);
            _bodyParts.Add(_upperRightArm);
            _upperRightArm.BodyType = BodyType.Dynamic;
            _upperRightArm.AngularDamping = LimbAngularDamping;
            _upperRightArm.Mass = mass;
            _upperRightArm.Rotation = -1.4f;
            _upperRightArm.Position = position + new Vector2(2f, 1.8f);

            _rightHand = BodyFactory.CreateCapsule(world, 0.3f, 0.3f, ArmDensity);
            _bodyParts.Add(_rightHand);
            _rightHand.BodyType = BodyType.Dynamic;
            _rightHand.AngularDamping = LimbAngularDamping;
            _rightHand.Mass = mass;
            _rightHand.Rotation = 1.4f;
            _rightHand.Position = position + new Vector2(4f, 2.7f);

            //Left Leg
            _lowerLeftLeg = BodyFactory.CreateCapsule(world, 1f, 0.5f, LegDensity);
            _bodyParts.Add(_lowerLeftLeg);
            _lowerLeftLeg.BodyType = BodyType.Dynamic;
            _lowerLeftLeg.AngularDamping = LimbAngularDamping;
            _lowerLeftLeg.Mass = mass;
            _lowerLeftLeg.Position = position + new Vector2(-0.6f, 8f);

            _upperLeftLeg = BodyFactory.CreateCapsule(world, 1f, 0.5f, LegDensity);
            _bodyParts.Add(_upperLeftLeg);
            _upperLeftLeg.BodyType = BodyType.Dynamic;
            _upperLeftLeg.AngularDamping = LimbAngularDamping;
            _upperLeftLeg.Mass = mass;
            _upperLeftLeg.Position = position + new Vector2(-0.6f, 6f);

            _leftFoot = BodyFactory.CreateCapsule(world, 0.5f, 0.5f, LegDensity);
            _bodyParts.Add(_leftFoot);
            _leftFoot.BodyType = BodyType.Dynamic;
            _leftFoot.AngularDamping = LimbAngularDamping;
            _leftFoot.Mass = mass;
            _leftFoot.Position = position + new Vector2(-0.6f, 8.5f);


            //Right Leg
            _lowerRightLeg = BodyFactory.CreateCapsule(world, 1f, 0.5f, LegDensity);
            _bodyParts.Add(_lowerRightLeg);
            _lowerRightLeg.BodyType = BodyType.Dynamic;
            _lowerRightLeg.AngularDamping = LimbAngularDamping;
            _lowerRightLeg.Mass = mass;
            _lowerRightLeg.Position = position + new Vector2(0.6f, 8f);

            _upperRightLeg = BodyFactory.CreateCapsule(world, 1f, 0.5f, LegDensity);
            _bodyParts.Add(_upperRightLeg);
            _upperRightLeg.BodyType = BodyType.Dynamic;
            _upperRightLeg.AngularDamping = LimbAngularDamping;
            _upperRightLeg.Mass = mass;
            _upperRightLeg.Position = position + new Vector2(0.6f, 6f);

            _rightFoot = BodyFactory.CreateCapsule(world, 0.5f, 0.5f, LegDensity);
            _bodyParts.Add(_rightFoot);
            _rightFoot.BodyType = BodyType.Dynamic;
            _rightFoot.AngularDamping = LimbAngularDamping;
            _rightFoot.Mass = mass;
            _rightFoot.Position = position + new Vector2(0.6f, 8.5f);

        }

        private void CreateJoints(World world)
        {
            const float dampingRatio = 1f;
            const float frequency = 25f;

            //head -> body
            DistanceJoint jHeadBody = new DistanceJoint(_head, _body,
                                                        new Vector2(0f, 1f),
                                                        new Vector2(0f, -2f));
            jHeadBody.CollideConnected = true;
            jHeadBody.DampingRatio = dampingRatio;
            jHeadBody.Frequency = frequency;
            jHeadBody.Length = 0.025f;
            world.AddJoint(jHeadBody);

            //lowerLeftArm -> upperLeftArm
            DistanceJoint jLeftArm = new DistanceJoint(_lowerLeftArm, _upperLeftArm,
                                                       new Vector2(0f, -1f),
                                                       new Vector2(0f, 1f));
            jLeftArm.CollideConnected = true;
            jLeftArm.DampingRatio = dampingRatio;
            jLeftArm.Frequency = frequency;
            jLeftArm.Length = 0.02f;
            world.AddJoint(jLeftArm);

            //lowerLeftArm -> leftHand

            //lowerLeftArm -> upperLeftArm
            DistanceJoint jLeftHand = new DistanceJoint(_leftHand,_lowerLeftArm,
                                                       new Vector2(0f, -0.5f),
                                                       new Vector2(0f, 0.5f));
            jLeftHand.CollideConnected = true;
            jLeftHand.DampingRatio = dampingRatio;
            jLeftHand.Frequency = frequency;
            jLeftHand.Length = 0.002f;
            world.AddJoint(jLeftHand);

            //upperLeftArm -> body
            DistanceJoint jLeftArmBody = new DistanceJoint(_upperLeftArm, _body,
                                                           new Vector2(0f, -1f),
                                                           new Vector2(-1f, -1.5f));
            jLeftArmBody.CollideConnected = true;
            jLeftArmBody.DampingRatio = dampingRatio;
            jLeftArmBody.Frequency = frequency;
            jLeftArmBody.Length = 0.02f;
            world.AddJoint(jLeftArmBody);

            //lowerRightArm -> upperRightArm
            DistanceJoint jRightArm = new DistanceJoint(_lowerRightArm, _upperRightArm,
                                                        new Vector2(0f, -1f),
                                                        new Vector2(0f, 1f));
            jRightArm.CollideConnected = true;
            jRightArm.DampingRatio = dampingRatio;
            jRightArm.Frequency = frequency;
            jRightArm.Length = 0.02f;
            world.AddJoint(jRightArm);

            //lowerRightArm -> rightHand
            DistanceJoint jRightHand = new DistanceJoint(_rightHand,_lowerRightArm,
                                                       new Vector2(0f, -0.5f),
                                                       new Vector2(0f, 0.5f));
            jRightHand.CollideConnected = true;
            jRightHand.DampingRatio = dampingRatio;
            jRightHand.Frequency = frequency;
            jRightHand.Length = 0.002f;
            world.AddJoint(jRightHand);


            //upperRightArm -> body
            DistanceJoint jRightArmBody = new DistanceJoint(_upperRightArm, _body,
                                                            new Vector2(0f, -1f),
                                                            new Vector2(1f, -1.5f));

            jRightArmBody.CollideConnected = true;
            jRightArmBody.DampingRatio = dampingRatio;
            jRightArmBody.Frequency = 25;
            jRightArmBody.Length = 0.02f;
            world.AddJoint(jRightArmBody);

            //lowerLeftLeg -> upperLeftLeg
            DistanceJoint jLeftLeg = new DistanceJoint(_lowerLeftLeg, _upperLeftLeg,
                                                       new Vector2(0f, -1.1f),
                                                       new Vector2(0f, 1f));
            jLeftLeg.CollideConnected = true;
            jLeftLeg.DampingRatio = dampingRatio;
            jLeftLeg.Frequency = frequency;
            jLeftLeg.Length = 0.05f;
            world.AddJoint(jLeftLeg);

            //lowerLeftleg -> leftFood
            DistanceJoint jLeftFoot = new DistanceJoint(_leftFoot,_lowerLeftLeg,
                                                       new Vector2(0f, -0.8f),
                                                       new Vector2(0f, 0.8f));
            jLeftFoot.CollideConnected = true;
            jLeftFoot.DampingRatio = dampingRatio;
            jLeftFoot.Frequency = frequency;
            jLeftFoot.Length = 0.05f;
            world.AddJoint(jLeftFoot);


            //upperLeftLeg -> body
            DistanceJoint jLeftLegBody = new DistanceJoint(_upperLeftLeg, _body,
                                                           new Vector2(0f, -1.1f),
                                                           new Vector2(-0.8f, 1.9f));
            jLeftLegBody.CollideConnected = true;
            jLeftLegBody.DampingRatio = dampingRatio;
            jLeftLegBody.Frequency = frequency;
            jLeftLegBody.Length = 0.02f;
            world.AddJoint(jLeftLegBody);

            //lowerRightleg -> upperRightleg
            DistanceJoint jRightLeg = new DistanceJoint(_lowerRightLeg, _upperRightLeg,
                                                        new Vector2(0f, -1.1f),
                                                        new Vector2(0f, 1f));
            jRightLeg.CollideConnected = true;
            jRightLeg.DampingRatio = dampingRatio;
            jRightLeg.Frequency = frequency;
            jRightLeg.Length = 0.05f;
            world.AddJoint(jRightLeg);

            //lowerRightLeg -> rightFoot
            DistanceJoint jRightFoot = new DistanceJoint(_rightFoot,_lowerRightLeg,
                                                        new Vector2(0f, -0.8f),
                                                        new Vector2(0f, 0.8f));
            jRightFoot.CollideConnected = true;
            jRightFoot.DampingRatio = dampingRatio;
            jRightFoot.Frequency = frequency;
            jRightFoot.Length = 0.05f;
            world.AddJoint(jRightFoot);


            //upperRightleg -> body
            DistanceJoint jRightLegBody = new DistanceJoint(_upperRightLeg, _body,
                                                            new Vector2(0f, -1.1f),
                                                            new Vector2(0.8f, 1.9f));
            jRightLegBody.CollideConnected = true;
            jRightLegBody.DampingRatio = dampingRatio;
            jRightLegBody.Frequency = frequency;
            jRightLegBody.Length = 0.02f;
            world.AddJoint(jRightLegBody);
        }

        private void CreateGFX()
        {
            AssetCreator creator = _screen.ScreenManager.Assets;
            _face = new Sprite(creator.CircleTexture(0.9f, MaterialType.Squares, Color.Gray, 1f));
            _torso = new Sprite(creator.TextureFromVertices(PolygonTools.CreateRoundedRectangle(2f, 4f, 0.5f, 0.7f, 2),
                                                             MaterialType.Squares, Color.LightSlateGray, 0.8f));

            _upperArm = new Sprite(creator.TextureFromVertices(PolygonTools.CreateCapsule(1.9f, 0.45f, 16),
                                                                MaterialType.Squares, Color.DimGray, 0.8f));
            _lowerArm = new Sprite(creator.TextureFromVertices(PolygonTools.CreateCapsule(1.9f, 0.45f, 16),
                                                                MaterialType.Squares, Color.DarkSlateGray, 0.8f));

            _upperLeg = new Sprite(creator.TextureFromVertices(PolygonTools.CreateCapsule(2f, 0.5f, 16),
                                                                MaterialType.Squares, Color.DimGray, 0.8f));
            _lowerLeg = new Sprite(creator.TextureFromVertices(PolygonTools.CreateCapsule(2f, 0.5f, 16),
                                                                MaterialType.Squares, Color.DarkSlateGray, 0.8f));

            _hand = new Sprite(creator.TextureFromVertices(PolygonTools.CreateCapsule(0.7f, 0.3f, 16),
                                                                MaterialType.Squares, Color.DimGray, 0.8f));

            _foot = new Sprite(creator.TextureFromVertices(PolygonTools.CreateCapsule(1.1f, 0.5f, 16),
                                                                MaterialType.Squares, Color.DimGray, 0.8f));
        }

        public void Draw()
        {
            SpriteBatch batch = _screen.ScreenManager.SpriteBatch;
            batch.Draw(_lowerLeg.Texture, ConvertUnits.ToDisplayUnits(_lowerLeftLeg.Position), null,
                        Color.White, _lowerLeftLeg.Rotation, _lowerLeg.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_lowerLeg.Texture, ConvertUnits.ToDisplayUnits(_lowerRightLeg.Position), null,
                        Color.White, _lowerRightLeg.Rotation, _lowerLeg.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_upperLeg.Texture, ConvertUnits.ToDisplayUnits(_upperLeftLeg.Position), null,
                        Color.White, _upperLeftLeg.Rotation, _upperLeg.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_upperLeg.Texture, ConvertUnits.ToDisplayUnits(_upperRightLeg.Position), null,
                        Color.White, _upperRightLeg.Rotation, _upperLeg.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_lowerArm.Texture, ConvertUnits.ToDisplayUnits(_lowerLeftArm.Position), null,
                        Color.White, _lowerLeftArm.Rotation, _lowerArm.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_lowerArm.Texture, ConvertUnits.ToDisplayUnits(_lowerRightArm.Position), null,
                        Color.White, _lowerRightArm.Rotation, _lowerArm.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_upperArm.Texture, ConvertUnits.ToDisplayUnits(_upperLeftArm.Position), null,
                        Color.White, _upperLeftArm.Rotation, _upperArm.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_upperArm.Texture, ConvertUnits.ToDisplayUnits(_upperRightArm.Position), null,
                        Color.White, _upperRightArm.Rotation, _upperArm.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_torso.Texture, ConvertUnits.ToDisplayUnits(_body.Position), null,
                        Color.White, _body.Rotation, _torso.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_face.Texture, ConvertUnits.ToDisplayUnits(_head.Position), null,
                        Color.White, _head.Rotation, _face.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_hand.Texture, ConvertUnits.ToDisplayUnits(_leftHand.Position), null,
                        Color.White, _leftHand.Rotation, _hand.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_hand.Texture, ConvertUnits.ToDisplayUnits(_rightHand.Position), null,
                        Color.White, _rightHand.Rotation, _hand.Origin, 1f, SpriteEffects.None, 0f);

            batch.Draw(_foot.Texture, ConvertUnits.ToDisplayUnits(_rightFoot.Position), null,
                        Color.White, _rightFoot.Rotation, _foot.Origin, 1f, SpriteEffects.None, 0f);
            batch.Draw(_foot.Texture, ConvertUnits.ToDisplayUnits(_leftFoot.Position), null,
                        Color.White, _leftFoot.Rotation, _foot.Origin, 1f, SpriteEffects.None, 0f);

            
        }

        public void RemoveFrom(World world)
        {
            foreach (var bodyPart in _bodyParts)
            {
                world.RemoveBody(bodyPart);
            }
        }
    }
}