// Licensed under the MIT License. See LICENSE in the project root for license information.

using NUnit.Framework;
using System;
using System.Text;

namespace Utilities.Extensions.Tests
{
    internal class TestFixture_01_NativeArrayExtensions
    {
        private const string rawString = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec leo mi, egestas ac scelerisque sed, tincidunt in urna. Morbi ultricies volutpat bibendum. Aliquam nec tortor pretium, convallis leo sit amet, vulputate neque. Morbi porttitor ornare sapien eget facilisis. Etiam interdum purus leo, ac imperdiet arcu vulputate vitae. In eget dolor mattis ipsum consectetur porttitor quis eu est. Fusce lobortis dui eu eros efficitur, non fermentum mi vehicula. Proin lacinia, purus non mattis tristique, felis dolor scelerisque magna, sit amet blandit ante eros sit amet ex. Mauris sit amet viverra ante, nec suscipit enim. Cras maximus, felis aliquet pellentesque lobortis, est nibh faucibus tellus, at bibendum eros mi id ligula.\r\n\r\nIn tincidunt imperdiet leo, id hendrerit nunc faucibus non. Proin accumsan consectetur malesuada. Sed quis suscipit nunc. Pellentesque ac elit ex. In porta dolor ac sapien efficitur dignissim. Curabitur malesuada erat eu diam pellentesque vehicula. Donec iaculis quis risus a feugiat. Nunc elementum tortor sit amet turpis mollis, id dignissim dolor tristique. Donec lobortis bibendum ligula, nec laoreet risus iaculis at. Suspendisse pulvinar, sem eu vehicula malesuada, tellus nisi egestas est, id feugiat urna turpis non velit. Nunc at rutrum purus. Aenean placerat metus vitae varius facilisis. Quisque mattis eget felis a dapibus. Donec ut elit nec erat auctor auctor. Etiam euismod ac tortor a ornare.\r\n\r\nPraesent nisl ante, sollicitudin a augue a, aliquet facilisis augue. Aenean congue varius turpis eget condimentum. Donec interdum ex eget odio volutpat, sit amet imperdiet lectus ornare. Nullam venenatis, tellus et fringilla bibendum, risus est fermentum mi, sed sagittis metus orci sed tortor. Sed odio metus, congue sed hendrerit luctus, luctus sed ex. Praesent porta, lorem sed tincidunt accumsan, ligula ligula hendrerit nunc, vel ornare lectus erat a libero. Morbi tristique imperdiet sem. Curabitur finibus orci at varius gravida. In lectus risus, sollicitudin quis convallis ac, aliquam in massa.\r\n\r\nCurabitur vitae nisi semper nibh tincidunt bibendum in et erat. Suspendisse semper laoreet lacus a tincidunt. Fusce id sem lacinia, mollis felis nec, auctor metus. Aliquam dictum sed augue at pharetra. Proin eu vulputate metus, quis vulputate ligula. Vivamus gravida vitae quam lobortis luctus. Duis sit amet tellus eu turpis rutrum feugiat. Nam sit amet efficitur libero, vestibulum vestibulum nisl. Nam et arcu tellus.\r\n\r\nDonec a pulvinar massa. Cras ultricies lorem quis sem ultrices, scelerisque aliquet elit pellentesque. Nunc dui turpis, interdum ac semper nec, pharetra ac quam. Nunc pharetra suscipit dignissim. Nunc pellentesque ex purus, nec consectetur purus ornare in. Etiam at metus libero. Sed dignissim mauris in scelerisque scelerisque. Aliquam in dignissim magna. Vivamus rhoncus massa sapien, eu dignissim quam euismod id. Suspendisse porttitor massa vitae aliquam sollicitudin. Curabitur justo quam, imperdiet in imperdiet et, dapibus sed mi.";

        [Test]
        public void Test_01_01_ConvertFromBase64String()
        {
            var rawStringBytes = Encoding.UTF8.GetBytes(rawString);
            var base64String = Convert.ToBase64String(rawStringBytes);
            using var nativeArray = NativeArrayExtensions.FromBase64String(base64String);

            Assert.AreEqual(rawStringBytes.Length, nativeArray.Length);

            for (var i = 0; i < rawStringBytes.Length; i++)
            {
                Assert.AreEqual(rawStringBytes[i], nativeArray[i]);
            }
        }

        [Test]
        public void Test_01_02_ConvertToBase64String()
        {
            var rawStringBytes = Encoding.UTF8.GetBytes(rawString);
            var base64String = Convert.ToBase64String(rawStringBytes);
            using var nativeArray = NativeArrayExtensions.FromBase64String(base64String);
            var convertedBase64String = NativeArrayExtensions.ToBase64String(nativeArray);
            Assert.AreEqual(base64String, convertedBase64String);
        }
    }
}
